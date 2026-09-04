using Kapibara.Core;
using WorkSetLinkFiles.ViewModels;

namespace WorkSetLinkFiles.Views;

public sealed partial class WorkSetLinkFilesView
{
    public WorkSetLinkFilesView(WorkSetLinkFilesViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        WorkSetLinkFilesViewModel.close = Close;
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}