using Kapibara.Core;
using Settings.ViewModels;

namespace Settings.Views;

public sealed partial class SettingsView
{
    public SettingsView(SettingsViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        Loaded += (s, e) =>
        {
            themeWatcherService.ApplyTheme();
        };
        
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}