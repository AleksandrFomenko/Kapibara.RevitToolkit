using System.IO;
using System.Text;

namespace ExporterModels.services;

public static class RevitServerDiscovery
{
    public static IReadOnlyList<string> GetServers(string revitVersion)
    {
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var configDir = Path.Combine(programData, "Autodesk", $"Revit Server {revitVersion}", "Config");
        var ini = Path.Combine(configDir, "RSN.ini");

        if (!File.Exists(ini)) return Array.Empty<string>();
        var lines = File.ReadAllLines(ini, Encoding.UTF8);
        var list = new List<string>();
        foreach (var raw in lines)
        {
            Console.WriteLine(raw);
            var s = raw?.Trim();
            if (string.IsNullOrEmpty(s)) continue;
            if (s.StartsWith("#") || s.StartsWith(";")) continue;
            list.Add(s);
        }

        return list;
    }
}