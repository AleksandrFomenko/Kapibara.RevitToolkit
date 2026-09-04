using EngineeringSystems.Commands;
using EngineeringSystems.Dialogs.AddAndRenameGroup.View;
using EngineeringSystems.Dialogs.AddAndRenameGroup.ViewModels;
using EngineeringSystems.services;
using EngineeringSystems.ViewModels;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EngineeringSystems.Dialogs.AddAndRenameGroup;

public static class AddAndRenameGroup
{
    public static GroupSystemsViewModel? HostViewModel { get; private set; }

    public static void Start(Variant variant, GroupSystemsViewModel hostViewModel)
    {
        HostViewModel = hostViewModel;
        var vm = AddAndRenameGroupViewModelsFabric.GetViewModel(variant,HostViewModel);
        var themeWatchServices = GroupSystems.Services.GetRequiredService<IThemeWatcherService>();
        var windowProvider = GroupSystems.Services.GetRequiredService<WindowProvider>();
        var view = new AddAndRenameGroupView(themeWatchServices, vm, windowProvider);
        view.ShowDialog();
    }
}

public static class AddAndRenameGroupViewModelsFabric
{
    public static GeneralViewModel GetViewModel(Variant variant, GroupSystemsViewModel hostViewModel)
    {
        return variant switch
        {
            Variant.Add => new AddGroupViewModel(hostViewModel),
            Variant.Rename => new RenameGroupViewModel(hostViewModel),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
        };
    }
    
}

public enum Variant
{
    Add,
    Rename
}