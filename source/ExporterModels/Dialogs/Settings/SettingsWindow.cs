using System.Windows;
using ExporterModels.Dialogs.Settings.Model;
using ExporterModels.Dialogs.Settings.View;
using ExporterModels.Dialogs.Settings.ViewModel;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationService = ExporterModels.services.ConfigurationService;

namespace ExporterModels.Dialogs.Settings;

public static class SettingsWindow
{
    public static SettingView Show(Window? owner, Action onClosed)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<SettingsModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<SettingView>();

        var provider = services.BuildServiceProvider();
        var vm = provider.GetService<SettingsViewModel>();
        var view = provider.GetService<SettingView>();
        var tws = provider.GetService<IThemeWatcherService>();

        if (vm != null) vm.OwnerView = view;
        if (view != null)
        {
            view.Owner = owner;
            
            view.Closed += (_, _) => onClosed?.Invoke();

            view.ShowDialog();
        }

        tws?.ApplyTheme();

        return view!;
    }
}