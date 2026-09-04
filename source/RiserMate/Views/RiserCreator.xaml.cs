using Kapibara.Core;
using RiserMate.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace RiserMate.Views;

public partial class RiserCreator : INavigableView<RizerCreatorViewModel>
{
    public RiserCreator(RizerCreatorViewModel viewModel, IThemeWatcherService themeService)
    {
        themeService.Watch(this);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }

    public RizerCreatorViewModel ViewModel { get; }
}