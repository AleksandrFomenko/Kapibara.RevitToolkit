namespace ViewByParameter.Models;

public partial class ElementsByParameter(string value, int count) : ObservableObject
{
    public event Action<bool>? SetCheck;
    public event Action? CheckButton; 

    [ObservableProperty] private bool _isChecked = true;
    [ObservableProperty] private bool _filterVisible ;
    public string Value { get; set; } = value;
    public int Count { get; set; } = count;


    partial void OnIsCheckedChanged(bool value)
    {
        OnCheckButton();
    }

    public virtual void OnSetCheck(bool res)
    {
        SetCheck?.Invoke(res);
    }


    protected virtual void OnCheckButton()
    {
        CheckButton?.Invoke();
    }
}