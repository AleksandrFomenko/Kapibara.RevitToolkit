using ExporterModels.Dialogs.AddConfiguration.ViewModel;
using Kapibara.Core;

namespace ExporterModels.Dialogs.AddConfiguration.View;

public partial class AddConfigurationView
{
    public AddConfigurationView(AddConfigurationViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        themeWatcherService.Watch(this);
        AddConfigurationViewModel.CloseWindow = Close;
        InitializeComponent();
    }

    public AddConfigurationViewModel ViewModel { get; }
}