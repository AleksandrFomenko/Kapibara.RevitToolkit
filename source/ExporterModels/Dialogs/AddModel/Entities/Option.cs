namespace ExporterModels.Dialogs.AddModel.Entities;

public partial class Option : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _visibleThreeList;
}