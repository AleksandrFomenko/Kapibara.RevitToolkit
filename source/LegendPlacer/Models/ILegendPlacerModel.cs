using System.Collections.ObjectModel;

namespace LegendPlacer.Models;

public interface ILegendPlacerModel
{
    public List<string>? GetLegends(string? legendName);
    public List<string?> GetCorners();
    public ObservableCollection<FolderItem> GetSheetItem();

    public void Execute(
        IEnumerable<FolderItem> folders,
        string legendName,
        string? position,
        double xChange,
        double yChange);
}