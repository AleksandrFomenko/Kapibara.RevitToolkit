using System.Collections.ObjectModel;
using ExporterModels.Dialogs.AddModel.Abstractions;
using ExporterModels.Dialogs.AddModel.Entities;

namespace ExporterModels.Dialogs.AddModel.Model;

public class ModelMock : IAddedModel
{
    public Task<ObservableCollection<ServerItem>> GetServersTreeAsync(CancellationToken ct = default)
    {
        var servers = new ObservableCollection<ServerItem>
        {
            BuildServer("172.16.222.122"),
            BuildServer("172.16.222.123")
        };
        return Task.FromResult(servers);
    }

    public ObservableCollection<Option> GetOptionAsync()
    {
        var options = new ObservableCollection<Option>
        {
            new() { Name = "С ревит сервера", VisibleThreeList = true },
            new() { Name = "из папки", VisibleThreeList = false }
        };

        return options;
    }

    private static ServerItem BuildServer(string server)
    {
        var root = new ServerItem { Name = server };

        var f1 = new FolderItem { Name = "Папка 1" };
        root.SubFolders.Add(f1);
        AddModel(f1, "Модель 1", "|Папка 1", server);
        AddModel(f1, "Модель 2", "|Папка 1", server);
        AddModel(f1, "Модель 3", "|Папка 1", server);
        AddModel(f1, "Модель 4", "|Папка 1", server);
        AddModel(f1, "Модель 5", "|Папка 1", server);

        var f2 = new FolderItem { Name = "Папка 2" };
        root.SubFolders.Add(f2);
        var f2sub = new FolderItem { Name = "Подпапка папки 2", Parent = f2 };
        f2.SubFolders.Add(f2sub);
        AddModel(f2sub, "Модель 1", "|Папка 2|Подпапка папки 2", server);
        AddModel(f2sub, "Модель 2", "|Папка 2|Подпапка папки 2", server);

        var f3 = new FolderItem { Name = "Папка 3" };
        root.SubFolders.Add(f3);
        var f3sub = new FolderItem { Name = "Подпапка папки 3", Parent = f3 };
        f3.SubFolders.Add(f3sub);
        var f3subsub = new FolderItem { Name = "Подпапка подпапки папки 3", Parent = f3sub };
        f3sub.SubFolders.Add(f3subsub);
        AddModel(f3subsub, "Модель 1", "|Папка 3|Подпапка папки 3|Подпапка подпапки папки 3", server);

        return root;
    }

    private static void AddModel(FolderItem parent, string modelName, string basePipe, string server)
    {
        parent.Sheets.Add(new SheetItem(modelName)
        {
            Parent = parent,
            PipePath = basePipe + "|" + modelName,
            RsnPath = $"rsn://{server}/{basePipe.TrimStart('|').Replace('|', '/')}/{modelName}"
        });
    }
}