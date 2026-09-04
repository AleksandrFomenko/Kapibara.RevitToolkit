using Kapibara.Core;
using SortingCategories.ViewModels;
using Wpf.Ui.Abstractions;


namespace SortingCategories.Views;

public sealed partial class SortingCategoriesView
{
    public SortingCategoriesView(
        SortingCategoriesViewModel viewModel,
        IThemeWatcherService themeWatcherService, 
        INavigationViewPageProvider serviceProvider
        )
    {
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        Loaded += (_, __) =>
        {
            RootNavigationView.SetPageProviderService(serviceProvider);
            RootNavigationView.Navigate(typeof(MainFamilies));
        };
        
        InitializeComponent();
    }
    
}