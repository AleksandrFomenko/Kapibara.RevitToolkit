using System.Collections.ObjectModel;
using ExporterModels.Dialogs.AddModel.Abstractions;
using ExporterModels.Dialogs.AddModel.Entities;
using ExporterModels.services;

namespace ExporterModels.Dialogs.AddModel.Model;

public sealed class Model : IAddedModel
{
    public async Task<ObservableCollection<ServerItem>> GetServersTreeAsync(CancellationToken ct = default)
    {
        var version = Context.UiApplication.Application.VersionNumber;
        var servers = RevitServerDiscovery.GetServers(version);


        var list = new ObservableCollection<ServerItem>();
        foreach (var s in servers)
        {
            using var client = new RevitServerClient(s, version);
            var serverNode = await client.LoadServerAsync(ct);
            list.Add(serverNode);
        }

        return list;
    }

    public ObservableCollection<Option> GetOptionAsync()
    {
        var options = new ObservableCollection<Option>
        {
            new() { Name = "С ревит сервера", VisibleThreeList = true },
            new() { Name = "С папки", VisibleThreeList = false }
        };

        return options;
    }
}