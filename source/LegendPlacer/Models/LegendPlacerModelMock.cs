using System.Collections.ObjectModel;

namespace LegendPlacer.Models;

public class LegendPlacerModelMock : ILegendPlacerModel
{
    // MOCK легенды в проекте
    public List<string>? GetLegends(string? legendName)
    {
        var legends = new List<string> { "Legend 1", "Legend 2", "Legend 3", 
            "Legend 1", "Legend 2", "Legend 3"
            ,"Legend 1", "Legend 2", "Legend 3"
        };
        
        return legends
            .Where(l => l.Contains(legendName!.ToLower()))
            .ToList();
    }

    public List<string?> GetCorners()
    {
        return ["Corners 1", "Corners 2", "Corners 3", "Corners 4"];
    }
    
    // MOCK организации браузера 
    public ObservableCollection<FolderItem> GetSheetItem()
    {
        return
        [
            new FolderItem
            {
                Name = "Папка 1",
                Sheets =
                {
                    new SheetItem("Лист 1", "1", 1),
                    new SheetItem("Лист 2", "2", 2),
                    new SheetItem("Лист 3", "3", 3),
                    new SheetItem("Лист 4", "4", 4),
                    new SheetItem("Лист 5", "5", 5)
                }
            },

            new FolderItem
            {
                Name = "Папка 2",
                SubFolders =
                {
                    new FolderItem
                    {
                        Name = "Подпапка папки 2",
                        Sheets =
                        {
                            new SheetItem("Лист 1", "1", 101),
                            new SheetItem("Лист 2", "2", 102),
                            new SheetItem("Лист 3", "3", 103),
                            new SheetItem("Лист 4", "4", 104),
                            new SheetItem("Лист 5", "5", 105)
                        }
                    }
                }
            },

            new FolderItem
            {
                Name = "Папка 3",
                SubFolders =
                {
                    new FolderItem
                    {
                        Name = "Подпапка папки 3",
                        SubFolders =
                        {
                            new FolderItem
                            {
                                Name = "Подпапка подпапки папки 3",
                                Sheets =
                                {
                                    new SheetItem("Лист 1", "1", 301),
                                    new SheetItem("Лист 2", "2", 302),
                                    new SheetItem("Лист 3", "3", 303)
                                }
                            }
                        }
                    }
                }
            }
        ];
    }

    public void Execute(IEnumerable<FolderItem> folders, string legendName, string? position, double xChange, double yChange)
    {
        throw new NotImplementedException();
    }
}