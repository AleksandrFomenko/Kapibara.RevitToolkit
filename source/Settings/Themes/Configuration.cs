using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Settings.Themes;

public sealed class Configuration
{
    public ApplicationTheme AppTheme { get; set; }
    public WindowBackdropType AppBackground { get; set; }
}
