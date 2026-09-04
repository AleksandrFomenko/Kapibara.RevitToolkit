using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using ExporterModels.Dialogs.AddConfiguration;
using ExporterModels.Dialogs.Settings.Entities;
using ExporterModels.Dialogs.Settings.Model;
using ExporterModels.Messaging;
using ExporterModels.services;
using Ookii.Dialogs.Wpf;

namespace ExporterModels.Dialogs.Settings.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigurationService _configService;
    private readonly SettingsModel _model;
    private static string Canon(string p) =>
        Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool PathEq(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) &&
        !string.IsNullOrWhiteSpace(b) &&
        string.Equals(Canon(a), Canon(b), StringComparison.OrdinalIgnoreCase);
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportConfigCommand))]
    private ObservableCollection<Configuration> _configurations = [];

    [ObservableProperty] private bool _isEnableButtons = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameConfigCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportConfigCommand))]
    private Configuration? _selectedConfiguration;

    public SettingsViewModel(SettingsModel model, ConfigurationService configService)
    {
        _model = model;
        _configService = configService;

        var settings = _configService.Load();
        Configurations = settings.Configurations ?? [];
        SelectedConfiguration = ResolveSelectedByPath(settings.SelectedConfigurationPath, Configurations);
    }

    public Window? OwnerView { get; set; } = new();

    partial void OnSelectedConfigurationChanged(Configuration? value)
    {
        var path = value?.Path;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Save();
            return;
        }
        
        Save();
        WeakReferenceMessenger.Default.Send(new SelectedConfigurationChangedMessage(path));
    }

    [RelayCommand]
    private async Task AddConfig()
    {
        await AddConfigurationWindow.ShowAsync(
            OwnerView,
            "Добавить конфигурацию",
            "Введите имя конфигурации...",
            "Добавить",
            SetEnableButtons,
            AddConfigurationCore);
        Save();
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private async Task RenameConfig()
    {
        await AddConfigurationWindow.ShowAsync(
            OwnerView,
            "Переименовать",
            "Введите новое имя конфигурации...",
            "Переименовать",
            SetEnableButtons,
            RenameConfigurationCore);
        Save();
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private void CopyConfig()
    {
        if (SelectedConfiguration == null) return;

        var newName = _model.FindAvailableName(Configurations, SelectedConfiguration.Name);

        var newPath = _configService.EnsureInternalConfig(newName);

        if (!string.IsNullOrWhiteSpace(SelectedConfiguration.Path) && File.Exists(SelectedConfiguration.Path))
            File.Copy(SelectedConfiguration.Path, newPath, true);
        
        var newConfig = new Configuration(newName, newPath);
        Configurations.Add(newConfig);
        SelectedConfiguration = newConfig;
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private void ExportConfig()
    {
        if (SelectedConfiguration?.Path is null || !File.Exists(SelectedConfiguration.Path))
            SelectedConfiguration!.Path = _configService.EnsureInternalConfig(SelectedConfiguration.Name);

        var dlg = new VistaSaveFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            AddExtension = true,
            DefaultExt = ".json",
            FileName = Path.GetFileName(SelectedConfiguration.Path),
            InitialDirectory = Path.GetDirectoryName(SelectedConfiguration.Path)
        };

        if (dlg.ShowDialog(OwnerView) == true)
        {
            _configService.ExportConfigFile(SelectedConfiguration.Path!, dlg.FileName);
        }
    }

    [RelayCommand]
    private void ImportConfig()
    {
        var dlg = new VistaOpenFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            Multiselect = false
        };

        if (dlg.ShowDialog(OwnerView) == true)
        {
            var file = Canon(dlg.FileName);
            var existingByPath = Configurations.FirstOrDefault(c => PathEq(c.Path, file));
            if (existingByPath is not null)
            {
                SelectedConfiguration = existingByPath;
                Save();
                return;
            }

            var name = Path.GetFileNameWithoutExtension(file);
            var uniqueName = MakeUniqueName(name, Configurations);

            var cfg = _model.AddConfigurations(Configurations, uniqueName, file);
            if (cfg is null) return;

            var app = _configService.Load();
            app.Configurations = Configurations;
            app.SelectedConfigurationPath = cfg.Path;
            _configService.Save(app);

            SelectedConfiguration = cfg;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private void DeleteConfig()
    {
        SelectedConfiguration = _model.DeleteConfigurations(Configurations, SelectedConfiguration!);
        Save();
    }

    private bool CanModifySelection()
    {
        return SelectedConfiguration is not null;
    }

    private void SetEnableButtons(bool value)
    {
        IsEnableButtons = value;
    }

    private void AddConfigurationCore(string name)
    {
        var path = _configService.EnsureInternalConfig(name);
           var cfg = _model.AddConfigurations(Configurations, name, path);
           if (cfg is null) return;
           SelectedConfiguration = cfg;
    }

    private void RenameConfigurationCore(string newName)
    {
        SelectedConfiguration = _model.RenameConfigurations(SelectedConfiguration!, newName);
    }

    private void Save()
    {
        _configService.Save(new AppSettings
        {
            Configurations = Configurations,
            SelectedConfigurationPath = SelectedConfiguration?.Path
        });
    }

    private static Configuration? ResolveSelectedByPath(string? savedPath, ObservableCollection<Configuration> list)
    {
        if (list is null || list.Count == 0) return null;
        if (string.IsNullOrWhiteSpace(savedPath)) return list.FirstOrDefault();
        return list.FirstOrDefault(c => string.Equals(c.Path, savedPath, StringComparison.OrdinalIgnoreCase))
               ?? list.FirstOrDefault();
    }

    private static string MakeUniqueName(string baseName, ObservableCollection<Configuration> list)
    {
        if (!list.Any(c => string.Equals(c.Name, baseName, StringComparison.Ordinal)))
            return baseName;

        var i = 1;
        while (true)
        {
            var candidate = $"{baseName} ({i})";
            if (!list.Any(c => string.Equals(c.Name, candidate, StringComparison.Ordinal)))
                return candidate;
            i++;
        }
    }
    
}