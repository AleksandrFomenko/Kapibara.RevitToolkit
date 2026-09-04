using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;


// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public sealed class ThemeWatcherService : IThemeWatcherService
{
    private static readonly List<FrameworkElement> ObservedElements = [];
    private const string ThemeFolderName = "KapibaraSettingsTheme";
    private const string ThemeFolderPath = "ThemeConfiguration";
    private PluginConfig<ThemeConfig>? Config { get; } = new(ThemeFolderName, ThemeFolderPath);

    
    public void Initialize()
    {
        UiApplication.Current.Resources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Kapibara.RevitToolKit.Core;component/Styles/App.Resources.xaml", 
                UriKind.Absolute)
        };

        ApplicationThemeManager.Changed += OnApplicationThemeManagerChanged;
    }
    
    public ApplicationTheme SelectedTheme => Config?.Data.AppTheme ?? ApplicationTheme.Light;
    public WindowBackdropType SelectedBackground => Config?.Data.AppBackground ?? WindowBackdropType.Mica;

    public void SetAppTheme(ApplicationTheme theme)
    {
        if (Config != null) Config.Data.AppTheme = theme;
        Config?.Save();
    }

    public void SetBackground(WindowBackdropType backdropType)
    {
        if (Config != null) Config.Data.AppBackground = backdropType;
        Config?.Save();
    }

    private void OnApplicationThemeManagerChanged(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
    {
        foreach (var frameworkElement in ObservedElements)
        {
            ApplicationThemeManager.Apply(frameworkElement);
            UpdateBackground(currentApplicationTheme);
            UpdateDictionary(frameworkElement);
        }
    }

    private static void UpdateDictionary(FrameworkElement frameworkElement)
    {
        var themedResources = frameworkElement.Resources.MergedDictionaries
            .Where(dictionary => dictionary.Source.OriginalString.Contains("WPF.UI;", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        frameworkElement.Resources.MergedDictionaries.Insert(0, UiApplication.Current.Resources.MergedDictionaries[0]);
        frameworkElement.Resources.MergedDictionaries.Insert(1, UiApplication.Current.Resources.MergedDictionaries[1]);

        foreach (var themedResource in themedResources)
            frameworkElement.Resources.MergedDictionaries.Remove(themedResource);
    }

    public void Watch(FrameworkElement frameworkElement)
    {
        ApplicationThemeManager.Apply(frameworkElement);
        frameworkElement.Loaded += OnWatchedElementLoaded;
        frameworkElement.Unloaded += OnWatchedElementUnloaded;
    }

    public void ApplyTheme()
    {
        var back = Config?.Data.AppBackground ?? WindowBackdropType.Mica;
        var theme = Config?.Data.AppTheme ?? ApplicationTheme.Dark;
        ApplicationThemeManager.Apply(theme, back);
        UpdateBackground(theme);
    }

    private void OnWatchedElementLoaded(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        ObservedElements.Add(element);

        if (element.Resources.MergedDictionaries[0].Source.OriginalString != UiApplication.Current.Resources.MergedDictionaries[0].Source.OriginalString)
        {
            ApplicationThemeManager.Apply(element);
            UpdateDictionary(element);
        }
        UpdateBackground(ApplicationThemeManager.GetAppTheme());
    }

    private void OnWatchedElementUnloaded(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        ObservedElements.Remove(element);
    }
    
    private void UpdateBackground(ApplicationTheme theme)
    {
        var backdrop = Config?.Data.AppBackground ?? WindowBackdropType.Mica;

        foreach (var window in ObservedElements.Select(Window.GetWindow).Distinct())
        {
            WindowBackgroundManager.UpdateBackground(window, theme, backdrop);
        }
    }
}