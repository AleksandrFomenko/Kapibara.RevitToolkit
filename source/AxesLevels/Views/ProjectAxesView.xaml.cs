using Kapibara.Core;

namespace AxesLevels.Views;

public sealed partial class ProjectAxesView
{
    public ProjectAxesView(IThemeWatcherService  themeWatcherService)
    {
        themeWatcherService.Watch(this);
        InitializeComponent();
    }
}