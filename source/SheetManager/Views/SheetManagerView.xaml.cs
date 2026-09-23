using Kapibara.Core;
using SheetManager.ViewModels;
using Wpf.Ui.Abstractions;

namespace SheetManager.Views;

public sealed partial class SheetManagerView
{
    public SheetManagerView(
        SheetManagerViewModel viewModel, 
        IThemeWatcherService watcherService, 
        INavigationViewPageProvider serviceProvider
        )
    {
        watcherService.Watch(this);
        DataContext = viewModel;
        
        Loaded += (_, __) =>
        {
            RootNavigationView.SetPageProviderService(serviceProvider);
            RootNavigationView.Navigate(typeof(CreateSheetsView));
        };
        
        InitializeComponent();
    }
}
