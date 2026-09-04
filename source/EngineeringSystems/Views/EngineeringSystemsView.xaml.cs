using EngineeringSystems.ViewModels;
using Kapibara.Core;

namespace EngineeringSystems.Views;

public sealed partial class EngineeringSystemsView
{
    public EngineeringSystemsView(EngineeringSystemsViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        EngineeringSystemsViewModel.Close = Close;
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        InitializeComponent();
    }
}