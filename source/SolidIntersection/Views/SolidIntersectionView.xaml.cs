using Kapibara.Core;
using SolidIntersection.ViewModels;

namespace SolidIntersection.Views;

public sealed partial class SolidIntersectionView
{
    public SolidIntersectionView(SolidIntersectionViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        SolidIntersectionViewModel.Close = Close;
        DataContext = viewModel;
        themeWatcherService.Watch(this);
        InitializeComponent();
    }
}