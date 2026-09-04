using System.Text.RegularExpressions;
using System.Windows.Input;
using ImportExcelByParameter.ViewModels;
using Kapibara.Core;


namespace ImportExcelByParameter.Views;

public sealed partial class ImportExcelByParameterView
{
    public ImportExcelByParameterView(
        ImportExcelByParameterViewModel viewModel,
        IThemeWatcherService themeWatcherService)
    {
        DataContext = viewModel;
        InitializeComponent();
        themeWatcherService.Watch(this);
        themeWatcherService.ApplyTheme();
        ImportExcelByParameterViewModel.CloseWindow = Close;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}