using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public static class ConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void CreateDir(string dllPath, string directoryName)
    {
        var dllDirectory = Path.GetDirectoryName(dllPath);
        var configDirectoryPath = Path.Combine(dllDirectory!, directoryName);
        if (!Directory.Exists(configDirectoryPath))
            Directory.CreateDirectory(configDirectoryPath);
    }

    public static void CreateEmptyJsonFile(string directoryPath, string fileName)
    {
        var filePath = Path.Combine(directoryPath, fileName);
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "{}");
    }

    public static T? LoadConfig<T>(string filePath) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл конфигурации не найден", filePath);

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File Not Found :{ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"An error occurred while deserializing the JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return null;
        }
    }

    public static bool SaveConfig<T>(string filePath, T obj)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(obj, JsonOptions);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save error: {ex.Message}");
            return false;
        }
    }
}