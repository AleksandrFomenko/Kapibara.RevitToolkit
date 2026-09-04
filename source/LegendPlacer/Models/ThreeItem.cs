using System.Collections.ObjectModel;

namespace LegendPlacer.Models;

public partial class FolderItem : ObservableObject
{
    [ObservableProperty]
    private bool _isChecked;
    
    [ObservableProperty]
    private string? _name;
    
    public ObservableCollection<FolderItem> SubFolders { get; } = [];
    public ObservableCollection<SheetItem> Sheets { get; } = [];
    
    public IEnumerable<object> AllChildren
        => SubFolders.Cast<object>()
            .Concat(Sheets);
    
    partial void OnIsCheckedChanged(bool value)
    {
        foreach (var sub in SubFolders)
            sub.IsChecked = value;
        foreach (var sheet in Sheets)
            sheet.IsChecked = value;
    }
}

public partial class SheetItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private string _name    = "";
    [ObservableProperty] private string _number  = "";
    [ObservableProperty] private long _elemId;

    public SheetItem(string name, string number, long elemId)
    {
        Name   = name;
        Number = number;
        ElemId  = elemId;
    }
}