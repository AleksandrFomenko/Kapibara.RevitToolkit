using Axes.ViewModels;
using Kapibara.Core;

namespace Axes.Views;

public sealed partial class AxesView
{
    public AxesView(AxesViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}