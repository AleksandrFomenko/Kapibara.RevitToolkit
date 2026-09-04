using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using ExporterModels.Abstractions;
using ExporterModels.Dialogs.AddModel;
using ExporterModels.Dialogs.AddProject;
using ExporterModels.Dialogs.AddProject.View;
using ExporterModels.Dialogs.Instruction;
using ExporterModels.Dialogs.RemoveModel;
using ExporterModels.Dialogs.RemoveModel.View;
using ExporterModels.Dialogs.RemoveProject;
using ExporterModels.Dialogs.Settings;
using ExporterModels.Dialogs.Settings.Entities;
using ExporterModels.Entities;
using ExporterModels.Messaging;
using ExporterModels.Progress;
using ExporterModels.RevitExporters;
using ExporterModels.services;
using Ookii.Dialogs.Wpf;
using Timer = System.Timers.Timer;

namespace ExporterModels.ViewModels;

public sealed partial class ExporterModelsViewModel : ObservableObject
{
    private readonly ConfigurationService _configService;
    private readonly Action<bool> _modelSelectedChangedHandler;

    private readonly Timer _saveTimer = new(300) { AutoReset = false };
    public readonly SynchronizationContext _ui;
    private readonly IWindowOwnerProvider _windowOwnerProvider;
    [ObservableProperty] private bool _addModelButtonEnable = true;

    [ObservableProperty] private bool _addProjectButtonEnable = true;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isCheckedDetach;
    [ObservableProperty] private bool _isCheckedNwc;
    [ObservableProperty] private bool _isCheckedResave;
    [ObservableProperty] private bool _openHelpButtonEnable = true;
    [ObservableProperty] private bool _progressIsIndeterminate;
    [ObservableProperty] private double _progressMaximum = 1;
    [ObservableProperty] private string _progressMessage = "";
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<Project> _projects = [];
    [ObservableProperty] private bool _removeModelButtonEnable = true;
    [ObservableProperty] private bool _removeProjectButtonEnable = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddModelCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveProjectCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteModelCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectNwcFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectDetachFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectResaveFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    private Project? _selectedProject;

    [ObservableProperty] private bool _settingsButtonEnable = true;
    [ObservableProperty] private string _titleWindow;

    private WorkspaceConfig _workspace = new();
    private string _workspacePath = string.Empty;

    public ExporterModelsViewModel(IWindowOwnerProvider windowOwnerProvider, ConfigurationService configService)
    {
        _windowOwnerProvider = windowOwnerProvider;
        _configService = configService;
        _modelSelectedChangedHandler = _ => ScheduleSave();
        _saveTimer.Elapsed += (_, _) => SaveWorkspaceNow();
        _ui = SynchronizationContext.Current ?? new SynchronizationContext();

        EnsureDefaultConfiguration();
        LoadWorkspaceOrInit();
        WireAutoSave(Projects);

        WeakReferenceMessenger.Default.Register<SelectedConfigurationChangedMessage>(
            this, (_, m) => SwitchWorkspace(m.Value));
    }


    private Action<Project>? OnProjectSelected { get; set; }

    partial void OnSelectedProjectChanged(Project? value)
    {
        if (value is not null)
            OnProjectSelected?.Invoke(value);

        _workspace.SelectedProjectName = value?.Name;
        ScheduleSave();
    }

    private void EnsureDefaultConfiguration()
    {
        var settings = _configService.Load();
        settings.Configurations ??= [];

        if (settings.Configurations.Count == 0)
        {
            const string name = "Configuration 1";
            var path = _configService.EnsureInternalConfig(name);
            settings.Configurations.Add(new Configuration(name, path));
            settings.SelectedConfigurationPath = path;
            _configService.Save(settings);
        }
        else if (string.IsNullOrWhiteSpace(settings.SelectedConfigurationPath))
        {
            settings.SelectedConfigurationPath = settings.Configurations.FirstOrDefault()?.Path;
            _configService.Save(settings);
        }
    }

    private void LoadWorkspaceOrInit()
    {
        var app = _configService.Load();
        _workspacePath = app.SelectedConfigurationPath ?? _configService.EnsureInternalConfig("Configuration 1");
        _workspace = _configService.LoadWorkspace(_workspacePath);

        Projects = _workspace.Projects ?? [];
        SelectedProject = ResolveSelectedProject(_workspace.SelectedProjectName, Projects);

        UpdateTitleWindow();
        ScheduleSave();
    }

    private void SwitchWorkspace(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath) || !File.Exists(newPath))
            return;

        var oldPath = _workspacePath;
        SaveWorkspaceTo(oldPath);

        UnwireAutoSave(Projects);

        _workspacePath = newPath;
        _workspace = _configService.LoadWorkspace(_workspacePath) ?? new WorkspaceConfig();
        Projects = _workspace.Projects ?? [];
        WireAutoSave(Projects);

        SelectedProject = ResolveSelectedProject(_workspace.SelectedProjectName, Projects);
        UpdateTitleWindow();
    }


    private static Project? ResolveSelectedProject(string? name, ObservableCollection<Project> list)
    {
        return string.IsNullOrWhiteSpace(name)
            ? list.FirstOrDefault()
            : list.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal));
    }

    private void WireAutoSave(ObservableCollection<Project> projects)
    {
        projects.CollectionChanged += ProjectsOnCollectionChanged;
        foreach (var p in projects) WireProject(p);
    }
    
    private void SaveWorkspaceTo(string path)
    {
        try
        {
            var snapshot = new WorkspaceConfig
            {
                Projects = Projects,
                SelectedProjectName = SelectedProject?.Name
            };

            _configService.SaveWorkspace(path, snapshot);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving workspace to {path}: {ex.Message}");
        }
    }

    private void UnwireAutoSave(ObservableCollection<Project> projects)
    {
        projects.CollectionChanged -= ProjectsOnCollectionChanged;
        foreach (var p in projects) UnwireProject(p);
    }

    private void ProjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Project p in e.OldItems)
                UnwireProject(p);

        if (e.NewItems != null)
            foreach (Project p in e.NewItems)
                WireProject(p);

        _workspace.Projects = Projects;
        ScheduleSave();
    }

    private void WireProject(Project p)
    {
        p.PropertyChanged += ProjectOnPropertyChanged;

        if (p.Models != null)
        {
            p.Models.CollectionChanged += ModelsOnCollectionChanged;
            foreach (var m in p.Models) WireModel(m);
        }
    }

    private void UnwireProject(Project p)
    {
        p.PropertyChanged -= ProjectOnPropertyChanged;

        if (p.Models != null)
        {
            p.Models.CollectionChanged -= ModelsOnCollectionChanged;
            foreach (var m in p.Models) UnwireModel(m);
        }
    }

    private void ProjectOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ScheduleSave();
    }

    private void ModelsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Model m in e.OldItems)
                UnwireModel(m);

        if (e.NewItems != null)
            foreach (Model m in e.NewItems)
                WireModel(m);

        ScheduleSave();
    }

    private void WireModel(Model m)
    {
        m.PropertyChanged += ModelOnPropertyChanged;
        m.IsSelectedChanged += _modelSelectedChangedHandler;
    }

    private void UnwireModel(Model m)
    {
        m.PropertyChanged -= ModelOnPropertyChanged;
        m.IsSelectedChanged -= _modelSelectedChangedHandler;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => ScheduleSave();

    private void ScheduleSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void SaveWorkspaceNow()
    {
        try
        {
            _workspace.Projects = Projects;
            _workspace.SelectedProjectName = SelectedProject?.Name;
            _configService.SaveWorkspace(_workspacePath, _workspace);
        }
        catch
        {
            Console.WriteLine("Error saving workspace");
        }
    }

    private string? GetCurrentConfigurationName()
    {
        var settings = _configService.Load();
        var path = _workspacePath;
        return settings.Configurations
            .FirstOrDefault(c => string.Equals(c.Path, path, StringComparison.OrdinalIgnoreCase))
            ?.Name;
    }

    private void UpdateTitleWindow()
    {
        var name = GetCurrentConfigurationName() ?? "unknown";
        TitleWindow = $"Export models, selection configurtation: {name}";
    }


    [RelayCommand]
    private void SetAllCheck()
    {
        if (SelectedProject is null) return;
        SelectedProject.Models.ToList().ForEach(m => m.IsSelected = SelectedProject.IsSelectedAll);
        SelectedProject.IsSelectedAll = !SelectedProject.IsSelectedAll;
    }

    [RelayCommand]
    private void AddProject()
    {
        AddProjectButtonEnable = false;
        var view = AddProjectWindow.Show(_windowOwnerProvider.GetOwner(), () => AddProjectButtonEnable = true);
        var vm = view.ViewModel;
        vm.AddProjectEvent += p =>
        {
            Projects.Add(p);
            SelectedProject = p;
            SaveWorkspaceTo(_workspacePath);
        };
    }

    [RelayCommand]
    private void OpenHelp()
    {
        OpenHelpButtonEnable = false;
        InstructionWindow.Show(_windowOwnerProvider.GetOwner(), () => OpenHelpButtonEnable = true);
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void DeleteModel()
    {
        if (SelectedProject is null) return;

        RemoveModelButtonEnable = false;
        var view = RemoveModelWindow.Show(_windowOwnerProvider.GetOwner(), () => RemoveModelButtonEnable = true);
        var vm = view.ViewModel;

        vm.SelectedModels = SelectedProject.SelectedModels;
        SelectedProject.CheckDeleteModelButton += _ => vm.DeleteModelCommand.NotifyCanExecuteChanged();
        OnProjectSelected += p => vm.SelectedModels = p.SelectedModels;

        vm.RemoveModel += models =>
        {
            foreach (var model in models?.ToList().Where(model => SelectedProject.Models.Contains(model))!)
                SelectedProject.Models.Remove(model);

            SelectedProject.SelectedModels!.Clear();
        };
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void AddModel()
    {
        if (SelectedProject is null) return;

        AddModelButtonEnable = false;
        var view = AddModelWindow.Show(_windowOwnerProvider.GetOwner(), () => AddModelButtonEnable = true);
        var vm = view.ViewModel;

        void OnModelsArrived(ObservableCollection<Model> models)
        {
            try
            {
                SelectedProject.AddModels(models);
            }
            finally
            {
                AddModelButtonEnable = true;
            }
        }

        vm.AddModelEvent += OnModelsArrived;
    }

    private bool CanAddModel()
    {
        return SelectedProject != null;
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void SelectNwcFolder()
    {
        if (SelectedProject is null) return;
        SelectedProject.NwcSavePath = OpenFolderDialog();
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void SelectResaveFolder()
    {
        if (SelectedProject is null) return;
        SelectedProject.ResaveSavePath = OpenFolderDialog();
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void SelectDetachFolder()
    {
        if (SelectedProject is null) return;
        SelectedProject.DetachSavePath = OpenFolderDialog();
    }

    private string OpenFolderDialog()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для сохранения",
            UseDescriptionForTitle = true
        };
        var result = dialog.ShowDialog();
        return result == true ? dialog.SelectedPath : string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private void RemoveProject()
    {
        if (SelectedProject is null) return;

        RemoveProjectButtonEnable = false;
        var view = RemoveProjectWindow.Show(_windowOwnerProvider.GetOwner(), () => RemoveProjectButtonEnable = true);
        var vm = view.ViewModel;

        vm.SelectedProject = SelectedProject;
        OnProjectSelected += p => vm.SelectedProject = p;
        vm.RemoveProject += p => Projects.Remove(p);
    }

    [RelayCommand]
    private void Settings()
    {
        SettingsButtonEnable = false;
        SettingsWindow.Show(_windowOwnerProvider.GetOwner(), () => SettingsButtonEnable = true);
    }


    private void MarkExportedModels(Project project, List<string> exportedPaths)
    {
        if (project?.Models is null || exportedPaths is null || exportedPaths.Count == 0) return;

        var now = DateTime.Now;
        var set = new HashSet<string>(exportedPaths, StringComparer.OrdinalIgnoreCase);

        _ui.Post(_ =>
        {
            foreach (var m in project.Models)
                if (!string.IsNullOrWhiteSpace(m.Path) && set.Contains(m.Path))
                    m.LastExportAt = now;
        }, null);
    }

    [RelayCommand(CanExecute = nameof(CanAddModel))]
    private async Task Execute()
    {
        if (SelectedProject is null) return;

        var paths = SelectedProject.Models?
            .Where(m => m.IsSelected && !string.IsNullOrWhiteSpace(m.Path))
            .Select(m => m.Path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        if (paths.Count == 0) return;

        var exporters = new List<(Func<List<string>, IProgress<ProgressInfo>, Task> exporter, string name)>
            {
                (IsCheckedNwc ? ExportNwcAsync : null, "NWC"),
                (IsCheckedDetach ? ExportDetachAsync : null, "Detach"),
                (IsCheckedResave ? ExportResaveAsync : null, "Resave")
            }
            .Where(x => x.exporter != null)
            .ToList();

        if (!exporters.Any()) return;

        var totalTasks = exporters.Sum(e => paths.Count);
        var progress = new UiProgress<ProgressInfo>(this, totalTasks);

        ProgressValue = 0;
        ProgressMaximum = totalTasks;
        ProgressIsIndeterminate = false;
        IsBusy = true;
        ProgressMessage = "Подготовка...";

        try
        {
            foreach (var (exporter, name) in exporters) await exporter(paths, progress);

            if (exporters.Any()) MarkExportedModels(SelectedProject, paths);
        }
        catch (Exception ex)
        {
            ProgressMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExecuteInRevitAsync(
        List<string> paths,
        Func<UIApplication, string, Task> action,
        IProgress<ProgressInfo> progress)
    {
        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            var fileName = Path.GetFileName(path);

            await Task.Delay(1);

            try
            {
                await Handlers.AsyncEventHandler.RaiseAsync(async app => { await action(app, path); });

                progress?.Report(new ProgressInfo(i + 1, paths.Count, fileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка при обработке {fileName}: {ex.Message}");
                progress?.Report(new ProgressInfo(i + 1, paths.Count, $"Ошибка: {fileName}"));
            }
        }
    }

    private async Task ExportNwcAsync(List<string> paths, IProgress<ProgressInfo> progress)
    {
        var directory = SelectedProject?.NwcSavePath;
        var badWorkset = SelectedProject?.WorksetName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory))
        {
            progress?.Report(new ProgressInfo(0, paths.Count, "Ошибка: не указана папка для NWC"));
            return;
        }

        var exporter = new NwcExporter();

        await ExecuteInRevitAsync(paths,
            async (app, path) => { await exporter.ExportSingleAsync(app, path, directory, badWorkset); }, progress);
    }

    private async Task ExportDetachAsync(List<string> paths, IProgress<ProgressInfo> progress)
    {
        var directory = SelectedProject?.DetachSavePath;
        var badWorkset = SelectedProject?.WorksetName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory))
        {
            progress?.Report(new ProgressInfo(0, paths.Count, "Ошибка: не указана папка для Detach"));
            return;
        }

        var exporter = new DetachExporter();

        await ExecuteInRevitAsync(paths,
            async (app, path) => { await exporter.ExportSingleAsync(app, path, directory, badWorkset); }, progress);
    }

    private async Task ExportResaveAsync(List<string> paths, IProgress<ProgressInfo> progress)
    {
        var directory = SelectedProject?.ResaveSavePath;

        if (string.IsNullOrWhiteSpace(directory))
        {
            progress?.Report(new ProgressInfo(0, paths.Count, "Ошибка: не указана папка для Resave"));
            return;
        }

        var exporter = new ResaveExporter();

        await ExecuteInRevitAsync(paths,
            async (app, path) => { await exporter.ExportSingleAsync(app, path, directory); }, progress);
    }
}