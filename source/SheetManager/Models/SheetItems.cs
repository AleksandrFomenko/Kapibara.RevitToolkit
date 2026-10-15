using System.Collections.ObjectModel;
namespace SheetManager.Models;

public sealed class TitleBlockItem
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;
    public string Name { get; set; } = string.Empty;
}
public partial class SheetItem : ObservableObject
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;
    public string Name { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    [ObservableProperty] private bool _isChecked;
}
public partial class FolderItem : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<FolderItem> SubFolders { get; } = new();
    public ObservableCollection<SheetItem> Sheets { get; } = new();
    public IEnumerable<object> AllChildren => SubFolders.Cast<object>().Concat(Sheets);
    [ObservableProperty] private bool _isChecked;
    partial void OnIsCheckedChanged(bool value) => SetChildrenChecked(value);
    public void SetChecked(bool value)
    {
        if (IsChecked != value) IsChecked = value;
        else SetChildrenChecked(value);
    }
    private void SetChildrenChecked(bool value)
    {
        foreach (var folder in SubFolders) folder.SetChecked(value);
        foreach (var sheet in Sheets) sheet.IsChecked = value;
    }
    public IEnumerable<SheetItem> GetSheets() => Sheets.Concat(SubFolders.SelectMany(f => f.GetSheets()));
}
