using Wpf.Ui.Controls;

namespace ExporterModels.Abstractions;

public interface IInfoBarService
{
    bool IsOpen { get; }
    string? Title { get; }
    string Message { get; }
    InfoBarSeverity Severity { get; }

    Task ShowInfoAsync(InfoBarSeverity severity, string title, string message);
}