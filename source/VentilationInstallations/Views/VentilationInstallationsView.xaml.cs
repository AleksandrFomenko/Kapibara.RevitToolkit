using Kapibara.Core;
using VentilationInstallations.ViewModels;

namespace VentilationInstallations.Views;

public sealed partial class VentilationInstallationsView
{
    public VentilationInstallationsView(
        VentilationInstallationsViewModel viewModel,
        IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        themeWatcherService.ApplyTheme();
        DataContext = viewModel;
        InitializeComponent();
    }
}
