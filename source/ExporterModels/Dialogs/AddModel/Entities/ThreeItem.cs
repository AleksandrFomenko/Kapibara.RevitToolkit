using System.Collections.ObjectModel;

namespace ExporterModels.Dialogs.AddModel.Entities;

public partial class ServerItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string _name = "";

    public ObservableCollection<FolderItem> SubFolders { get; } = [];
    public ObservableCollection<SheetItem> Sheets { get; } = [];

    public IEnumerable<object> AllChildren => SubFolders.Cast<object>().Concat(Sheets);

    partial void OnIsCheckedChanged(bool value)
    {
        foreach (var f in SubFolders) f.SetCheckedFromParent(value);
        foreach (var s in Sheets) s.IsChecked = value;
    }
}

public partial class FolderItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string? _name;

    public FolderItem? Parent { get; set; }
    public ObservableCollection<FolderItem> SubFolders { get; } = new();
    public ObservableCollection<SheetItem> Sheets { get; } = new();

    public IEnumerable<object> AllChildren => SubFolders.Cast<object>().Concat(Sheets);

    partial void OnIsCheckedChanged(bool value)
    {
        foreach (var f in SubFolders) f.SetCheckedFromParent(value);
        foreach (var s in Sheets) s.IsChecked = value;
        Parent?.RecomputeFromChildren2State();
    }

    internal void SetCheckedFromParent(bool value)
    {
        if (_isChecked != value)
        {
            _isChecked = value;
            OnPropertyChanged(nameof(IsChecked));
        }

        foreach (var f in SubFolders) f.SetCheckedFromParent(value);
        foreach (var s in Sheets) s.IsChecked = value;
    }

    public void RecomputeFromChildren2State()
    {
        if (SubFolders.Count == 0 && Sheets.Count == 0) return;

        var allChecked = SubFolders.All(f => f.IsChecked) && Sheets.All(s => s.IsChecked);
        if (_isChecked != allChecked)
        {
            _isChecked = allChecked;
            OnPropertyChanged(nameof(IsChecked));
        }

        Parent?.RecomputeFromChildren2State();
    }
}

public partial class SheetItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string _name = "";


    [ObservableProperty] private string _pipePath = "";
    [ObservableProperty] private string _rsnPath = "";

    public SheetItem()
    {
    }

    public SheetItem(string name)
    {
        _name = name;
    }

    public FolderItem? Parent { get; set; }

    partial void OnIsCheckedChanged(bool value)
    {
        Parent?.RecomputeFromChildren2State();
    }
}