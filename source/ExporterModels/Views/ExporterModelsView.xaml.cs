using ExporterModels.ViewModels;
using Kapibara.Core;

namespace ExporterModels.Views;

public sealed partial class ExporterModelsView
{
    public ExporterModelsView(ExporterModelsViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}