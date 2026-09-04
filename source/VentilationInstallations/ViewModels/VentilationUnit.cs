namespace VentilationInstallations.ViewModels;

public sealed partial class VentilationUnit : ObservableObject
{
    public VentilationUnit(long id, string familyName, string levelName, string systemName, string workset)
    {
        Id = id;
        FamilyName = familyName;
        LevelName = levelName;
        SystemName = systemName;
        Workset = workset;
    }

    public long Id { get; }
    public string FamilyName { get; }
    public string LevelName { get; }
    public string SystemName { get; }
    public string Workset { get; }

    [ObservableProperty]
    private bool _isChecked = true;

    public event EventHandler? CheckedChanged;

    partial void OnIsCheckedChanged(bool value) => CheckedChanged?.Invoke(this, EventArgs.Empty);
}