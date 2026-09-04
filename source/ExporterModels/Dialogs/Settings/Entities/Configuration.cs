namespace ExporterModels.Dialogs.Settings.Entities;

public partial class Configuration(string name, string? path) : ObservableObject
{
    [ObservableProperty] private string _name = name;
    [ObservableProperty] private string? _path = path;
}