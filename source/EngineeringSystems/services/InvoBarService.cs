using Wpf.Ui.Controls;

namespace EngineeringSystems.services;

public partial class InfoBarService : ObservableObject
{
    private const int DurationMs = 3000;

    [ObservableProperty] private bool _isOpen;

    [ObservableProperty] private string _message = string.Empty;

    [ObservableProperty] private InfoBarSeverity _severity;

    [ObservableProperty] private string? _title;

    public async Task ShowInfoAsync(InfoBarSeverity severity, string title, string message)
    {
        Title = title;
        Message = message;
        Severity = severity;
        IsOpen = true;

        await Task.Delay(DurationMs);

        IsOpen = false;
    }
}