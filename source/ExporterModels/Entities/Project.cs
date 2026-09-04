using System.Collections.ObjectModel;

namespace ExporterModels.Entities;

public partial class Project : ObservableObject
{
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _detachSavePath;
    [ObservableProperty] private bool _isSelectedAll = true;
    [ObservableProperty] private ObservableCollection<Model> _models;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _nwcSavePath;
    [ObservableProperty] private string _resaveSavePath;
    [ObservableProperty] private ObservableCollection<Model>? _selectedModels = [];
    [ObservableProperty] private string _worksetName;

    public Project(string name, string description, ObservableCollection<Model> models)
    {
        _name = name;
        _description = description;
        _models = models;
        foreach (var model in models) model.IsSelectedChanged += SetSelectionModel;
    }

    public event Action<ObservableCollection<Model>?> CheckDeleteModelButton;

    partial void OnSelectedModelsChanged(ObservableCollection<Model>? value)
    {
        OnCheckDeleteModelButton(value);
    }

    private void SetSelectionModel(bool isSelected)
    {
        if (SelectedModels == null) return;
        foreach (var selectedModel in SelectedModels) selectedModel.IsSelected = isSelected;
    }

    private void OnCheckDeleteModelButton(ObservableCollection<Model>? obj)
    {
        CheckDeleteModelButton?.Invoke(obj);
    }

    public void AddModels(IEnumerable<Model> newModels)
    {
        if (newModels == null) return;
        var count = Models.Count;
        var i = 1;
        foreach (var m in newModels)
        {
            if (m == null) continue;

            if (Models.Any(x => string.Equals(x.Path, m.Path, StringComparison.OrdinalIgnoreCase)))
                continue;
            m.Number = count + i;
            i++;
            m.IsSelectedChanged += SetSelectionModel;

            m.IsSelected = true;

            Models.Add(m);
        }

        OnCheckDeleteModelButton(SelectedModels);
    }
}

public partial class Model(int number, string name, string path, string section) : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private DateTime? _lastExportAt;
    [ObservableProperty] private string _name = name;
    [ObservableProperty] private int _number = number;
    [ObservableProperty] private string _path = path;
    [ObservableProperty] private string _section = section;

    public event Action<bool>? IsSelectedChanged;

    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(value);
    }
}