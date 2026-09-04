using System.Windows;
using ExporterModels.Dialogs.AddConfiguration.View;
using ExporterModels.Dialogs.AddConfiguration.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Kapibara.Core;

namespace ExporterModels.Dialogs.AddConfiguration;

public static class AddConfigurationWindow
{
    public static async Task<AddConfigurationView?> ShowAsync(
        Window? owner,
        string? title,
        string? placeHolderText,
        string? buttonContent,
        Action<bool>? onStateChanged,
        Action<string> run)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton(provider => new AddConfigurationViewModel(title, placeHolderText, buttonContent, run));
        services.AddSingleton<AddConfigurationView>();

        var provider = services.BuildServiceProvider();
        var view = provider.GetService<AddConfigurationView>();
        if (view != null) view.Owner = owner;
        var tws = provider.GetService<IThemeWatcherService>();
        
        onStateChanged?.Invoke(false);

        var tcs = new TaskCompletionSource<bool>();

        view!.Closed += (s, e) => { tcs.SetResult(true); };

        view?.Show();
        tws?.ApplyTheme();

        await tcs.Task;

        onStateChanged?.Invoke(true);

        return view;
    }
}