using System.Collections.ObjectModel;
using ExporterModels.Dialogs.AddModel.Entities;

namespace ExporterModels.Dialogs.AddModel.Abstractions;

public interface IAddedModel
{
    Task<ObservableCollection<ServerItem>> GetServersTreeAsync(CancellationToken ct = default);
    ObservableCollection<Option> GetOptionAsync();
}