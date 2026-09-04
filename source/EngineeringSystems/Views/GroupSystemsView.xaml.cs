using EngineeringSystems.ViewModels;
using Kapibara.Core;

namespace EngineeringSystems.Views;

public partial class GroupSystemsView
{
    public GroupSystemsView(IThemeWatcherService themeWatcherService, GroupSystemsViewModel viewModel)
    {
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        GroupSystemsViewModel.CloseWindow = Close;
        Closed += (_, _) => viewModel.AfterClose();
        InitializeComponent();
    }
}