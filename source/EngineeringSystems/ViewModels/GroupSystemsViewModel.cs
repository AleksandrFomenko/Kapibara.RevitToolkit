using System.Collections.ObjectModel;
using System.ComponentModel;
using EngineeringSystems.Dialogs.AddAndRenameGroup;
using EngineeringSystems.Dialogs.AddAndRenameGroup.EventArgs;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.ViewModels.Entities;
using Group = EngineeringSystems.ViewModels.Entities.Group;

namespace EngineeringSystems.ViewModels;

public sealed partial class GroupSystemsViewModel : ObservableObject
{
    private readonly IGroupSystemsModel _model;
    
    [ObservableProperty]
    private ObservableCollection<Group> _systemGroups = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameGroupCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteGroupCommand))]
    private Group? _selectedSystemGroups;
    
    private ObservableCollection<Group> _allSystemGroups = [];

    [ObservableProperty] 
    private ObservableCollection<EngineeringSystem> _engineeringSystems = [];

    [ObservableProperty] 
    private string _filterGroupSystems = string.Empty;

    [ObservableProperty] 
    private string? _filterSystems = string.Empty;

    [ObservableProperty]
    private GroupSystemsOptions _selectedOption;
    
    [ObservableProperty]
    private ObservableCollection<GroupSystemsOptions> _groupSystemsOptions = [];
    
    [ObservableProperty] 
    private List<string> _userParameters = [];
    
    [ObservableProperty]
    private string _selectedUserParameter;
    
    [ObservableProperty]
    private bool _createView;

    public static Action? CloseWindow;

    public GroupSystemsViewModel(IGroupSystemsModel model)
    { 
        _model = model; 

        _allSystemGroups = _model.GetGroupSystems() ?? [];
        
        SystemGroups.Clear();
        foreach (var g in _allSystemGroups.Where(g => !g.IsDeleted))
            SystemGroups.Add(g);

        EngineeringSystems = _model.GetProjectSystems() ?? [];
        
        GroupSystemsOptions = new ObservableCollection<GroupSystemsOptions>(
            Enum.GetValues(typeof(GroupSystemsOptions))
                .Cast<GroupSystemsOptions>()
        );

        SelectedOption = GroupSystemsOptions.First();
        UserParameters = _model.GetUserParameters();
        SelectedUserParameter = _model.GetSelectedParameter();
    }

    partial void OnFilterGroupSystemsChanged(string value)
    {
        IEnumerable<Group> source;

        if (string.IsNullOrWhiteSpace(value))
        {
            source = _allSystemGroups.Where(g => !g.IsDeleted);
        }
        else
        {
            source = _allSystemGroups
                .Where(g => !g.IsDeleted &&
                            !string.IsNullOrEmpty(g.Name) &&
                            g.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
        }
        
        SystemGroups.Clear();
        foreach (var g in source)
            SystemGroups.Add(g);
    }
    
    partial void OnFilterSystemsChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            EngineeringSystems = _model.GetProjectSystems() ?? [];
            return;
        }

        var allSystems = _model.GetProjectSystems() ?? [];

        EngineeringSystems = new ObservableCollection<EngineeringSystem>(
            allSystems.Where(x =>
                !string.IsNullOrEmpty(x.NameSystem) &&
                x.NameSystem.Contains(value!))
        );
    }
    
    public void HandleGroupNameRequested(object? sender, GroupNameEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Name))
        {
            e.Result = GroupNameDialogResult.Cancelled;
            return;
        }

        var exists = SystemGroups.Any(g =>
            string.Equals(g.Name, e.Name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            e.Result = GroupNameDialogResult.NameIsExist;
            return;
        }

        var newGroup = new Group(e.Name, new ObservableCollection<EngineeringSystem>());

        _allSystemGroups.Add(newGroup);
        SystemGroups.Add(newGroup);
        
        if (e.Name.Length <= 3)
            e.Result = GroupNameDialogResult.TooShort; 
        else
            e.Result = GroupNameDialogResult.Success;
    }
    
    public void HandleGroupRenameRequested(object? sender, GroupNameEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Name))
        {
            e.Result = GroupNameDialogResult.Cancelled;
            return;
        }

        var name = e.Name.Trim();

        var exists = SystemGroups.Any(g =>
            string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            e.Result = GroupNameDialogResult.NameIsExist;
            return;
        }
        
        var selected = SelectedSystemGroups;

        if (selected != null)
            selected.Name = name;

        if (name.Length <= 3)
            e.Result = GroupNameDialogResult.TooShort;
        else
            e.Result = GroupNameDialogResult.Success;
    }

    public void AfterClose()
    {
        var toSave = new ObservableCollection<Group>(
            _allSystemGroups.Where(g => !g.IsDeleted));

        _model.AfterClose(toSave, SelectedUserParameter);
    }
    
    [RelayCommand]
    private void CheckAll()
    {
        foreach (var g in SystemGroups)
            g.IsChecked = true;
    }

    [RelayCommand]
    private void UnCheckAll()
    {
        foreach (var g in SystemGroups)
            g.IsChecked = false;
    }

    [RelayCommand]
    private void AddGroup() => AddAndRenameGroup.Start(Variant.Add, this);

    [RelayCommand(CanExecute = nameof(CanAddRenameGroup))]
    private void RenameGroup() => AddAndRenameGroup.Start(Variant.Rename, this);
    
    [RelayCommand(CanExecute = nameof(CanAddRenameGroup))]
    private void DeleteGroup()
    {
        if (SelectedSystemGroups is null)
            return;

        var groupToRemove = SelectedSystemGroups;
        
        if (groupToRemove.Systems is { Count: > 0 })
        {
            var existingIds = new HashSet<long>(EngineeringSystems.Select(s => s.SystemId));

            foreach (var system in groupToRemove.Systems)
            {
                if (system.SystemId == 0)
                    continue;

                if (existingIds.Add(system.SystemId))
                {
                    EngineeringSystems.Add(system);
                }
            }
        }
        
        groupToRemove.IsDeleted = true;
        
        SelectedSystemGroups = null;
        
        var source = _allSystemGroups.Where(g => !g.IsDeleted);

        SystemGroups.Clear();
        foreach (var g in source)
            SystemGroups.Add(g);
    }
    
    private bool CanAddRenameGroup() => SelectedSystemGroups != null;

    private void Start()
    {
        var result = SelectedOption == ViewModels.GroupSystemsOptions.All
            ? SystemGroups
            : SystemGroups.Where(g => g.IsChecked);
        _model.Execute(result.ToList(), SelectedUserParameter, CreateView);
    }
    
    [RelayCommand]
    private void Execute() => Start();

    [RelayCommand]
    private void ExecuteAndClose()
    {
        Start();
        CloseWindow?.Invoke();
    }
}

public enum GroupSystemsOptions
{
    [Description("Обработать все группы")] All,
    [Description("Обработать выбранные группы")] Selected,
}
