using Kapibara.Core;
using LevelByFloor.ViewModels;

namespace LevelByFloor.Views;

public sealed partial class LevelByFloorView
{
    public LevelByFloorView(LevelByFloorViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        LevelByFloorViewModel.Close = Close;
        InitializeComponent();
    }
}