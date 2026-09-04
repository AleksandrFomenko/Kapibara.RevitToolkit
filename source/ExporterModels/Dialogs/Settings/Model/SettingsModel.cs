using System.Collections.ObjectModel;
using ExporterModels.Dialogs.Settings.Entities;

namespace ExporterModels.Dialogs.Settings.Model;

public class SettingsModel
{
    public Configuration AddConfigurations(ObservableCollection<Configuration> configurations, string name,
        string? path)
    {
        var existingConfig = configurations.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));


        if (existingConfig != null) return existingConfig;

        var cfg = new Configuration(name, path);
        configurations.Add(cfg);
        return cfg;
    }

    public Configuration RenameConfigurations(Configuration configuration, string name)
    {
        configuration.Name = name;
        return configuration;
    }

    public Configuration DeleteConfigurations(ObservableCollection<Configuration> configurations,
        Configuration configuration)
    {
        configurations.Remove(configuration);
        return configurations.LastOrDefault();
    }

    public string FindAvailableName(ObservableCollection<Configuration> configurations, string baseName)
    {
        var newName = $"{baseName} copy";

        if (!configurations.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) return newName;

        return FindAvailableNameRecursive(configurations, baseName, 1);
    }

    private string FindAvailableNameRecursive(ObservableCollection<Configuration> configurations,
        string baseName, int copyNumber)
    {
        var newName = $"{baseName} copy";
        if (copyNumber > 1) newName += $" ({copyNumber})";
        if (!configurations.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) return newName;
        return FindAvailableNameRecursive(configurations, baseName, copyNumber + 1);
    }
}