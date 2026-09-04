using Kapibara.Core;
using SheetManager.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace SheetManager.Views;

public partial class PrintSheetsView : INavigableView<PrintSheetsViewModel>
{
    public PrintSheetsViewModel ViewModel { get; }
    
    public PrintSheetsView(PrintSheetsViewModel viewModel, IThemeWatcherService themeService)
    {
        themeService.Watch(this);
        ViewModel = viewModel;
        InitializeComponent();
    }
}