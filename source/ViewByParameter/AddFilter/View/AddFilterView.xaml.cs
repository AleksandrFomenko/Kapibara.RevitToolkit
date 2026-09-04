using Kapibara.Core;
using ViewByParameter.AddFilter.ViewModels;

namespace ViewByParameter.AddFilter.View;

public partial class AddFilterView
{
    public AddFilterViewModel ViewModel { get; set; }
    
    public AddFilterView(AddFilterViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        ViewModel.CloseWindow += Close;
        themeWatcherService.Watch(this);
        InitializeComponent();
    }
}