using System.Windows;
using ExporterModels.Abstractions;
using ExporterModels.Dialogs.RemoveProject.View;
using ExporterModels.Dialogs.RemoveProject.ViewModel;
using ExporterModels.services;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ExporterModels.Dialogs.RemoveProject;

public static class RemoveProjectWindow
{
    public static RemoveProjectView Show(Window? owner, Action onClosed)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<IInfoBarService, InfoBarService>();
        services.AddSingleton<RemoveProjectViewModel>();
        services.AddSingleton<RemoveProjectView>();

        var provider = services.BuildServiceProvider();

        var view = provider.GetService<RemoveProjectView>();
        var tws = provider.GetService<IThemeWatcherService>();
        tws?.ApplyTheme();
        view.Owner = owner;
        view.Show();
        view.Closed += (_, _) => { onClosed?.Invoke(); };
        return view;
    }
}