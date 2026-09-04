using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using ExporterModels.Dialogs.AddModel.Entities;


namespace ExporterModels.services;

internal sealed class AdminFolderEntry
{
    public string Name { get; set; } = "";
    public bool HasContents { get; set; }
}

internal sealed class AdminFileEntry
{
    public string Name { get; set; } = "";
}

internal sealed class AdminContentResponse
{
    public string Path { get; set; } = "";
    public List<AdminFolderEntry> Folders { get; set; } = new();
    public List<AdminFileEntry> Files { get; set; } = new();
    public List<AdminFileEntry> Models { get; set; } = new();
}

public sealed class RevitServerClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _revitVersion;
    private readonly string _server;

    public RevitServerClient(string serverNameOrIP, string revitVersion, TimeSpan? timeout = null,
        HttpMessageHandler handler = null)
    {
        _server = serverNameOrIP ?? throw new ArgumentNullException(nameof(serverNameOrIP));
        _revitVersion = revitVersion ?? throw new ArgumentNullException(nameof(revitVersion));

#if NET48
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        ServicePointManager.DefaultConnectionLimit = Math.Max(20, ServicePointManager.DefaultConnectionLimit);
#endif
        var effectiveHandler = handler ?? new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _http = new HttpClient(effectiveHandler, handler == null);
        _http.Timeout = timeout ?? TimeSpan.FromSeconds(60);
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Console.WriteLine(
            $"[RS/INIT] server='{_server}', version='{_revitVersion}', timeout='{_http.Timeout}', accept='{string.Join(",", _http.DefaultRequestHeaders.Accept)}'");
    }

    public void Dispose()
    {
        _http.Dispose();
    }

    public async Task<ServerItem> LoadServerAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[RS/LOAD] START server='{_server}'");
        var sw = Stopwatch.StartNew();

        var root = new ServerItem { Name = _server };

        var rootContents = await GetAdminContentsAsync("|", ct);
        Console.WriteLine(
            $"[RS/LOAD] root contents: folders={rootContents.Folders.Count}, files={rootContents.Files.Count}, models={rootContents.Models.Count}");

        foreach (var f in rootContents.Folders)
        {
            Console.WriteLine($"[RS/LOAD] +folder '{f.Name}', hasContents={f.HasContents}");
            var folder = new FolderItem { Name = f.Name };
            root.SubFolders.Add(folder);

            if (f.HasContents)
                await BuildFolderAsync(CombinePipe("|", f.Name), folder, ct, 1);
        }

        foreach (var m in EnumerateModels(rootContents))
        {
            var pipe = CombinePipe("|", m);
            var rsn = BuildRsnPath(_server, pipe);
            Console.WriteLine($"[RS/LOAD] +model '{m}' pipe='{pipe}' rsn='{rsn}' (root)");
            root.Sheets.Add(new SheetItem(m) { Parent = null, PipePath = pipe, RsnPath = rsn });
        }

        sw.Stop();
        Console.WriteLine(
            $"[RS/LOAD] DONE server='{_server}' subFolders={root.SubFolders.Count} models={root.Sheets.Count} in {sw.Elapsed}");
        return root;
    }

    private async Task BuildFolderAsync(string folderBarePipe, FolderItem parent, CancellationToken ct, int depth)
    {
        var indent = new string(' ', depth * 2);
        Console.WriteLine($"{indent}[RS/RECURSE] ENTER pipe='{folderBarePipe}' parent='{parent.Name}'");

        AdminContentResponse contents;
        try
        {
            contents = await GetAdminContentsAsync(folderBarePipe, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{indent}[RS/RECURSE] ERROR on pipe='{folderBarePipe}': {ex.Message}");
            throw;
        }

        Console.WriteLine(
            $"{indent}[RS/RECURSE] contents: folders={contents.Folders.Count}, files={contents.Files.Count}, models={contents.Models.Count}");

        foreach (var m in EnumerateModels(contents))
        {
            var pipe = CombinePipe(folderBarePipe, m);
            var rsn = BuildRsnPath(_server, pipe);
            Console.WriteLine($"{indent}[RS/RECURSE] +model '{m}' pipe='{pipe}' rsn='{rsn}'");
            parent.Sheets.Add(new SheetItem(m) { Parent = parent, PipePath = pipe, RsnPath = rsn });
        }

        foreach (var f in contents.Folders)
        {
            Console.WriteLine($"{indent}[RS/RECURSE] +folder '{f.Name}' hasContents={f.HasContents}");
            var childFolder = new FolderItem { Name = f.Name, Parent = parent };
            parent.SubFolders.Add(childFolder);

            if (f.HasContents)
            {
                var childPath = CombinePipe(folderBarePipe, f.Name);
                await BuildFolderAsync(childPath, childFolder, ct, depth + 1);
            }
            else
            {
                Console.WriteLine($"{indent}[RS/RECURSE] skip empty '{f.Name}'");
            }
        }

        Console.WriteLine(
            $"{indent}[RS/RECURSE] EXIT '{parent.Name}' sub={parent.SubFolders.Count} models={parent.Sheets.Count}");
    }

    private async Task<AdminContentResponse> GetAdminContentsAsync(string folderPipeBare, CancellationToken ct)
    {
        var encodedPipe = EncodePipePath(folderPipeBare);
        var url = BuildUrl("/" + encodedPipe + "/contents");
        Console.WriteLine($"[REST] GET {url}");
        Console.WriteLine($"[REST] pipe(raw)='{folderPipeBare}' -> pipe(encoded)='|{encodedPipe.TrimStart('|')}'");

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        EnrichHeaders(req);
        Console.WriteLine($"[REST] headers: User-Name='{Environment.UserName}', Machine='{Environment.MachineName}'");

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        var ctype = resp.Content.Headers.ContentType?.ToString() ?? "<null>";
        var charset = resp.Content.Headers.ContentType?.CharSet ?? "<null>";
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var encName = resp.Content.Headers.ContentType?.CharSet;
        Encoding enc;
        try
        {
            enc = !string.IsNullOrEmpty(encName) ? Encoding.GetEncoding(encName!) : Encoding.UTF8;
        }
        catch
        {
            enc = Encoding.UTF8;
        }

        var json = enc.GetString(bytes);

        Console.WriteLine($"[REST] Status={(int)resp.StatusCode}({resp.StatusCode}) CT='{ctype}' Len={json.Length}");
        Console.WriteLine($"[REST] Body preview: {Preview(json, 600)}");

        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine($"[REST] ERROR {resp.StatusCode} on {url}");
            throw new HttpRequestException($"Status {(int)resp.StatusCode}: {json}");
        }

        try
        {
            var obj = JsonSerializer.Deserialize<AdminContentResponse>(json);
            if (obj == null) throw new InvalidOperationException("Deserialized to null");

            Console.WriteLine(
                $"[REST] Parsed OK: folders={obj.Folders?.Count ?? 0}, files={obj.Files?.Count ?? 0}, models={obj.Models?.Count ?? 0}");
            return obj;
        }
        catch (Exception parseEx)
        {
            Console.WriteLine($"[REST] PARSE ERROR: {parseEx.Message}");
            throw;
        }
    }

    private static IEnumerable<string> EnumerateModels(AdminContentResponse c)
    {
        if (c.Models is { Count: > 0 })
        {
            Console.WriteLine($"[RS/MODELS] using 'Models' list ({c.Models.Count})");
            foreach (var m in c.Models)
            {
                var name = m?.Name ?? string.Empty;
                if (IsModelName(name)) yield return name;
            }

            yield break;
        }

        if (c.Files is { Count: > 0 })
        {
            Console.WriteLine($"[RS/MODELS] fallback to 'Files' list ({c.Files.Count})");
            foreach (var f in c.Files)
            {
                var name = f?.Name ?? string.Empty;
                if (IsModelName(name)) yield return name;
            }
        }
        else
        {
            Console.WriteLine("[RS/MODELS] no models/files detected");
        }
    }

    private static bool IsModelName(string name)
    {
        var n = name?.ToLowerInvariant() ?? "";
        return n.EndsWith(".rvt") || n.EndsWith(".rfa");
    }

    private string BuildUrl(string encodedInfo)
    {
        var u = $"http://{_server}/RevitServerAdminRESTService{_revitVersion}/AdminRESTService.svc{encodedInfo}";
        return u;
    }

    private static string EncodePipePath(string barePipe)
    {
        var p = barePipe;
        if (string.IsNullOrWhiteSpace(p)) p = "|";
        if (!p.StartsWith("|")) p = "|" + p.TrimStart('/').Replace('/', '|');

        var parts = p.Split(new[] { '|' }, StringSplitOptions.None);
        var sb = new StringBuilder();
        sb.Append('|');
        for (var i = 1; i < parts.Length; i++)
        {
            if (i > 1) sb.Append('|');
            var seg = parts[i];
            if (string.IsNullOrEmpty(seg)) continue;
            sb.Append(Uri.EscapeDataString(seg));
        }

        var res = sb.ToString();
        Console.WriteLine($"[PATH] raw='{barePipe}' -> encoded='{res}'");
        return res;
    }

    private static string CombinePipe(string baseBare, string name)
    {
        if (string.IsNullOrEmpty(name)) return baseBare;
        if (baseBare == "|") return "|" + name;
        return baseBare + "|" + name;
    }

    private static string BuildRsnPath(string server, string barePipe)
    {
        var p = barePipe.StartsWith("|") ? barePipe.Substring(1) : barePipe;
        var slash = p.Replace('|', '/');
        return $"rsn://{server}/{slash}";
    }

    private static void EnrichHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("User-Name", Environment.UserName ?? "Unknown");
        request.Headers.TryAddWithoutValidation("User-Machine-Name", Environment.MachineName ?? "Unknown");
        request.Headers.TryAddWithoutValidation("Operation-GUID", Guid.NewGuid().ToString());
    }

    private static string Preview(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "<empty>";
        if (s.Length <= max) return s;
        return s.Substring(0, max) + $" ... (len={s.Length})";
    }
}