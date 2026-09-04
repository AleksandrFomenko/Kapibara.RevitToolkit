using System.Windows.Controls;
using Kapibara.Core;
using SheetManager.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace SheetManager.Views;

public partial class CreateSheetsView : INavigableView<CreateSheetsViewModel>
{
    public CreateSheetsViewModel ViewModel { get; }
    
    public CreateSheetsView(CreateSheetsViewModel viewModel, IThemeWatcherService themeService)
    {
        themeService.Watch(this);
        ViewModel = viewModel;
        InitializeComponent();
    }
}