using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using ExporterModels.Dialogs.Settings.Entities;
using ExporterModels.Entities;

namespace ExporterModels.services;

public sealed class ConfigurationService
{
    private const string RootDirName = "ExportModels";
    private const string ConfigName = "config.json";

    private static readonly string DllPath = Assembly.GetExecutingAssembly().Location;
    private static readonly string DllDirectory = Path.GetDirectoryName(DllPath)!;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks =
        new(StringComparer.OrdinalIgnoreCase);

    private static SemaphoreSlim GetGate(string path) =>
        _locks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));

    private readonly string _rootDir;
    private readonly string _configFilePath;
    private readonly string _configsStoreDir;

    public ConfigurationService()
    {
        _rootDir = Path.Combine(DllDirectory, RootDirName);
        _configFilePath = Path.Combine(_rootDir, ConfigName);
        _configsStoreDir = Path.Combine(_rootDir, "Configs");

        Directory.CreateDirectory(_rootDir);
        Directory.CreateDirectory(_configsStoreDir);
        
        var gate = GetGate(_configFilePath);
        gate.Wait();
        try
        {
            if (!File.Exists(_configFilePath))
            {
                var defaults = new AppSettings();
                SafeWriteJson(_configFilePath, defaults);
            }
        }
        finally
        {
            gate.Release();
        }
    }
    

    public void Save(AppSettings settings)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));

        var gate = GetGate(_configFilePath);
        gate.Wait();
        try
        {
            Directory.CreateDirectory(_rootDir);
            SafeWriteJson(_configFilePath, settings);
        }
        finally
        {
            gate.Release();
        }
    }

    public AppSettings Load()
    {
        var gate = GetGate(_configFilePath);
        gate.Wait();
        try
        {
            Directory.CreateDirectory(_rootDir);

            if (!File.Exists(_configFilePath))
            {
                var defaults = new AppSettings();
                SafeWriteJson(_configFilePath, defaults);
                return defaults;
            }

            try
            {
                var text = SafeReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<AppSettings>(text, _jsonOptions)
                       ?? new AppSettings();
            }
            catch
            {
                var defaults = new AppSettings();
                SafeWriteJson(_configFilePath, defaults);
                return defaults;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public string GetPath() => _configFilePath;

    public string GetConfigsStoreDir() => _configsStoreDir;

    public string BuildInternalConfigPath(string configName)
    {
        var baseName = MakeSafeFileName(string.IsNullOrWhiteSpace(configName)
            ? "config"
            : configName.Trim());

        return Path.Combine(_configsStoreDir, baseName + ".json");
    }

    public string EnsureInternalConfig(string configName)
    {
        var path = BuildInternalConfigPath(configName);
        var gate = GetGate(path);

        gate.Wait();
        try
        {
            Directory.CreateDirectory(_configsStoreDir);

            if (!File.Exists(path))
            {
                SafeWriteAllText(path, "{}");
            }
        }
        finally
        {
            gate.Release();
        }

        return path;
    }

    public WorkspaceConfig LoadWorkspace(string workspacePath)
    {
        if (string.IsNullOrWhiteSpace(workspacePath))
            return new WorkspaceConfig();

        var gate = GetGate(workspacePath);
        gate.Wait();
        try
        {
            var dir = Path.GetDirectoryName(workspacePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(workspacePath))
                return new WorkspaceConfig();

            try
            {
                var text = SafeReadAllText(workspacePath);
                if (string.IsNullOrWhiteSpace(text))
                    return new WorkspaceConfig();

                return JsonSerializer.Deserialize<WorkspaceConfig>(text, _jsonOptions)
                       ?? new WorkspaceConfig();
            }
            catch
            {
                return new WorkspaceConfig();
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public void SaveWorkspace(string workspacePath, WorkspaceConfig data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrWhiteSpace(workspacePath)) throw new ArgumentException("Путь не задан", nameof(workspacePath));

        var gate = GetGate(workspacePath);
        gate.Wait();
        try
        {
            var dir = Path.GetDirectoryName(workspacePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            SafeWriteJson(workspacePath, data);
        }
        finally
        {
            gate.Release();
        }
    }

    public void ExportConfigFile(string sourcePath, string destPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new FileNotFoundException("Исходный конфигурационный файл не найден.", sourcePath);

        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.Copy(sourcePath, destPath, true);
    }
    

    private static string MakeSafeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static void SafeWriteJson<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        SafeWriteAllText(path, json);
    }

    private static void SafeWriteAllText(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tempPath = path + ".tmp";

        using (var fs = new FileStream(
                   tempPath,
                   FileMode.Create,
                   FileAccess.Write,
                   FileShare.None))
        using (var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
        {
            sw.Write(content);
            sw.Flush();
            fs.Flush(true);
        }

        if (File.Exists(path))
        {
            try
            {
                File.Replace(tempPath, path, destinationBackupFileName: null);
            }
            catch
            {
                File.Delete(path);
                File.Move(tempPath, path);
            }
        }
        else
        {
            File.Move(tempPath, path);
        }
    }

    private static string SafeReadAllText(string path)
    {
        using var fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        using var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return sr.ReadToEnd();
    }
}

public sealed class AppSettings
{
    public ObservableCollection<Configuration> Configurations { get; set; } = [];
    public string? SelectedConfigurationPath { get; set; }
}

public sealed class WorkspaceConfig
{
    public ObservableCollection<Project> Projects { get; set; } = [];
    public string? SelectedProjectName { get; set; }
}
