using System.Windows;

namespace EngineeringSystems.ViewModels.Entities;

public class Options(
    string name,
    double width,
    double height,
    GridLength firstColumnWidth,
    GridLength secondColumnWidth,
    bool flag)
{
    public string NameOpt { get; set; } = name;
    public double Width { get; } = width;
    public double Height { get; } = height;
    public GridLength FirstColumnWidth { get; } = firstColumnWidth;
    public GridLength SecondColumnWidth { get; } = secondColumnWidth;
    public bool Flag { get; set; } = flag;
}

public class SystemParameters(string name)
{
    public string Name { get; set; } = name;
}

public class FilterOption(string name, string revitApiMethodName)
{
    public string Name { get; set; } = name;
    public string RevitApiMethodName { get; set; } = revitApiMethodName;
}

public sealed partial class EngineeringSystem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string _nameSystem = string.Empty;
    [ObservableProperty] private long _systemId;
    [ObservableProperty] private string _cutSystemName = string.Empty;
}