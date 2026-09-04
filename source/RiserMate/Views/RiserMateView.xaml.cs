using Kapibara.Core;
using RiserMate.ViewModels;
using Wpf.Ui.Abstractions;

namespace RiserMate.Views;

public sealed partial class RiserMateView
{
    public RiserMateView(
        RiserMateViewModel viewModel,
        IThemeWatcherService themeWatcherService,
        INavigationViewPageProvider serviceProvider
    )
    
    {
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        Loaded += (_, __) =>
        {
            RootNavigationView.SetPageProviderService(serviceProvider);
            RootNavigationView.Navigate(typeof(RiserCreator));
        };
        
        InitializeComponent();
    }
}