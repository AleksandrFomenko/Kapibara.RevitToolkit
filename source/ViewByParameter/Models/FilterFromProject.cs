namespace ViewByParameter.Models;

public sealed partial class FilterFromProject(string name) : ObservableObject
{
    public event Action<bool>? SetAllChecked;
    public event Action? CheckButton;
    
    [ObservableProperty] private bool _isChecked;
    public string Name {get; set; } = name;
    public bool IsEnabled {get; set; } = true;
    public bool IsVisible {get; set; } = true;


    partial void OnIsCheckedChanged(bool value)
    {
        OnCheckButton();
    }

    public void OnSetAllChecked(bool obj)
    {
        SetAllChecked?.Invoke(obj);
    }

    public void OnCheckButton()
    {
        CheckButton?.Invoke();
    }
}