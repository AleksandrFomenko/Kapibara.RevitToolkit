using KapibaraUI.Services.Appearance;
using VentilationInstallations.ViewModels;

namespace VentilationInstallations.Views;

public sealed partial class VentilationInstallationsView
{
    public VentilationInstallationsView(
        VentilationInstallationsViewModel viewModel,
        IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        themeWatcherService.SetConfigTheme();
        DataContext = viewModel;
        InitializeComponent();
    }
}