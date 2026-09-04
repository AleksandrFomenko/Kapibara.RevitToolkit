using EngineeringSystems.Dialogs.AddAndRenameGroup.ViewModels;
using EngineeringSystems.services;
using Kapibara.Core;

namespace EngineeringSystems.Dialogs.AddAndRenameGroup.View;

public partial class AddAndRenameGroupView
{
    public AddAndRenameGroupView(
        IThemeWatcherService themeWatcherService, 
        GeneralViewModel viewModel, 
        WindowProvider windowProvider)
    {
        Owner = windowProvider.WindowOwner;
        themeWatcherService.Watch(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}