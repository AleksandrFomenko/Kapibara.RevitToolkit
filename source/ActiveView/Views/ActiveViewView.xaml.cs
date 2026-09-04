using ActiveView.ViewModels;
using Kapibara.Core;

namespace ActiveView.Views;

public sealed partial class ActiveViewView
{
    public ActiveViewView(ActiveViewViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}