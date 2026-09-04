using System.Windows;
using ExporterModels.Abstractions;
using ExporterModels.Dialogs.RemoveModel.View;
using ExporterModels.Dialogs.RemoveModel.ViewModel;
using ExporterModels.services;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ExporterModels.Dialogs.RemoveModel;

public static class RemoveModelWindow
{
    public static RemoveModelView? Show(Window? owner, Action onClosed)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<IInfoBarService, InfoBarService>();
        services.AddSingleton<RemoveModelViewModel>();
        services.AddSingleton<RemoveModelView>();

        var provider = services.BuildServiceProvider();

        var view = provider.GetService<RemoveModelView>();
        if (view != null) return null;
        var tws = provider.GetService<IThemeWatcherService>();
        tws?.ApplyTheme();
        view!.Owner = owner;
        view.Show();
        tws?.ApplyTheme();
        view.Closed += (_, _) => { onClosed?.Invoke(); };
        return view;
    }
}