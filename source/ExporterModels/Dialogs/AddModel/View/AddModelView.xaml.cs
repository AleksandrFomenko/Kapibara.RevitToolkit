using ExporterModels.Dialogs.AddModel.ViewModel;
using Kapibara.Core;

namespace ExporterModels.Dialogs.AddModel.View;

public partial class AddModelView
{
    public AddModelView(AddModelViewModel viewModel, IThemeWatcherService theme)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        theme.Watch(this);
        InitializeComponent();
    }

    public AddModelViewModel ViewModel { get; }
}