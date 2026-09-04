using Kapibara.Core;
using SortingCategories.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace SortingCategories.Views;

public partial class MainFamilies : INavigableView<SortingCategoriesViewModel>
{
    public SortingCategoriesViewModel ViewModel { get; }
    public MainFamilies(SortingCategoriesViewModel viewModel, IThemeWatcherService themeService)
    {
        themeService.Watch(this);
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}