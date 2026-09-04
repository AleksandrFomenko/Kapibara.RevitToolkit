using Kapibara.Core;
using SortingCategories.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace SortingCategories.Views;

public partial class SubFamilies :  INavigableView<SubFamiliesViewModel>
{
    public SubFamiliesViewModel ViewModel { get; }
    public SubFamilies(SubFamiliesViewModel viewModel,IThemeWatcherService themeService)
    {
        ViewModel = viewModel;
        DataContext = this;
        themeService.Watch(this);
        InitializeComponent();
    }
}