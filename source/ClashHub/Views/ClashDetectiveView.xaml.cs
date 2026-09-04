using Kapibara.Core;
using ClashDetectiveViewModel = ClashHub.ViewModels.ClashDetectiveViewModel;

namespace ClashHub.Views;

public sealed partial class ClashDetectiveView
{
    public ClashDetectiveView(ClashDetectiveViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
        
    }
}