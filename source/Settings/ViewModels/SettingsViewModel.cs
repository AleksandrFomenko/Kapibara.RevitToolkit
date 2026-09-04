using System.Collections.ObjectModel;
using Kapibara.Core;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Settings.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _titleName = "Настройки";
    [ObservableProperty] private string _themeTitle = "Выбор темы";
    [ObservableProperty] private string _backgroundTitle = "Выбор фона";
    [ObservableProperty] private ObservableCollection<ApplicationTheme> _themes = null!;
    [ObservableProperty] private ApplicationTheme _selectedTheme;
    [ObservableProperty] private ObservableCollection<WindowBackdropType> _backgrounds = null!;
    [ObservableProperty] private WindowBackdropType _selectedBackground;
    
    private readonly IThemeWatcherService _themeWatcherService;

    
    public SettingsViewModel(IThemeWatcherService themeWatcherService)
    {
        _themeWatcherService = themeWatcherService;

        Themes = new ObservableCollection<ApplicationTheme>(
            ((ApplicationTheme[])Enum.GetValues(typeof(ApplicationTheme)))
            .Where(t => t != ApplicationTheme.Auto && t != ApplicationTheme.Unknown));
        
        Backgrounds = new ObservableCollection<WindowBackdropType>(
            (WindowBackdropType[])Enum.GetValues(typeof(WindowBackdropType)));
        
        try
        {
            SelectedTheme = _themeWatcherService.SelectedTheme;
            SelectedBackground = _themeWatcherService.SelectedBackground;
        }
        catch
        {
            SelectedTheme = ApplicationTheme.Light;
            SelectedBackground = WindowBackdropType.Mica;
        }
    }
    
    partial void OnSelectedThemeChanged(ApplicationTheme value)
    {
        _themeWatcherService.SetAppTheme(value);
        _themeWatcherService.ApplyTheme();
    }

    partial void OnSelectedBackgroundChanged(WindowBackdropType value)
    {
        _themeWatcherService.SetBackground(value);
        _themeWatcherService.ApplyTheme();
    }
}