using ExporterModels.Dialogs.Settings.ViewModel;
using Kapibara.Core;

namespace ExporterModels.Dialogs.Settings.View;

public partial class SettingView
{
    public SettingView(SettingsViewModel viewModel, IThemeWatcherService theme)
    {
        DataContext = viewModel;
        ViewModel = viewModel;
        theme.Watch(this);
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}