using System.Collections.ObjectModel;
using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;
using Group = EngineeringSystems.ViewModels.Entities.Group;

namespace EngineeringSystems.Model.Abstractions;

public interface IGroupSystemsModel
{
    public ObservableCollection<Group> GetGroupSystems();
    public ObservableCollection<EngineeringSystem> GetProjectSystems();
    public void AfterClose(ObservableCollection<Group>? groups, string userParameter);
    public List<string> GetUserParameters();
    public string GetSelectedParameter();
    public void Execute(List<Group> systemGroups, string userParameter, bool CreateView3D);
}