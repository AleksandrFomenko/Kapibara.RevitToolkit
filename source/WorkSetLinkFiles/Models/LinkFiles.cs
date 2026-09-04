namespace WorkSetLinkFiles.Models;

public partial class LinkFiles : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string? _revitModelName;
    [ObservableProperty] private string? _worksetName;
    [ObservableProperty] private string? _prefix;
    [ObservableProperty] private string? _suffix;
    
    public event Action? IsCheckedChanged;
    partial void OnIsCheckedChanged(bool value) =>  IsCheckedChanged?.Invoke();

}