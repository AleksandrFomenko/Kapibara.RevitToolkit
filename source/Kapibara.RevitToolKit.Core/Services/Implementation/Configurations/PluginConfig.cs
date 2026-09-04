using System.IO;
using System.Reflection;



// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public class PluginConfig<T> where T : class, new()
{
    private readonly string _configFilePath;

    public T Data { get; private set; }

    public PluginConfig(string directoryName, string configName)
    {
        var dllPath = Assembly.GetExecutingAssembly().Location;
        var dllDir = Path.GetDirectoryName(dllPath);
        
        ConfigurationService.CreateDir(dllPath, directoryName);
        
        var dirPath = Path.Combine(dllDir!, directoryName);
        _configFilePath = Path.Combine(dirPath, configName);
        
        if (!File.Exists(_configFilePath))
        {
            Data = new T();
            Save();
        }
        else
        {
            Data = ConfigurationService.LoadConfig<T>(_configFilePath) ?? new T();
        }
    }
    public void Save() => ConfigurationService.SaveConfig(_configFilePath, Data);

}