namespace RiserMate.Entities;

public partial class HeatingRiser(string name) : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string _name = name;

    public event EventHandler<HeatingRiser>? ClickSelect;
    public event EventHandler<HeatingRiser>? ClickShow3D;

    [RelayCommand]
    private void Select() => OnClickSelect(this);
    
    [RelayCommand]
    private void Show3D() => OnClickShow3D(this);

    protected virtual void OnClickSelect(HeatingRiser e) => ClickSelect?.Invoke(this, e);
    protected virtual void OnClickShow3D(HeatingRiser e) => ClickShow3D?.Invoke(this, e);

}