using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

// ReSharper disable CheckNamespace

namespace Kapibara.Core;


public interface IThemeWatcherService
{
    ApplicationTheme SelectedTheme { get; }
    WindowBackdropType SelectedBackground { get; }
    void Initialize();
    void Watch(FrameworkElement frameworkElement);
    void ApplyTheme();

    public void SetAppTheme(ApplicationTheme theme);
    public void SetBackground(WindowBackdropType background);
}