using System.Windows;
using Kapibara.Core;
using Marking.ViewModels;

namespace Marking.Views;

public sealed partial class MarkingView
{
    public MarkingView(MarkingViewModel viewModel, IThemeWatcherService themeService)
    {
        themeService.Watch(this);
        DataContext = viewModel;
        themeService.ApplyTheme();
        SourceInitialized += (s, e) => themeService.ApplyTheme();
        viewModel.MinimizeWindow += () =>  WindowState = WindowState.Minimized;
        viewModel.Model.SendMaximize += () =>  WindowState = WindowState.Normal;
        InitializeComponent();
        
    }
}