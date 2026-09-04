using System.Collections.ObjectModel;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;
using Group = EngineeringSystems.ViewModels.Entities.Group;

namespace EngineeringSystems.Model.Mock;

public class GroupSystemsMockModel : IGroupSystemsModel
{
    public ObservableCollection<Group> GetGroupSystems()
    {
        ObservableCollection<Group> list = new();

        var systems = new List<EngineeringSystem>
        {
            new() { NameSystem = "ПВ1", SystemId = 1111 },
            new() { NameSystem = "П1", SystemId = 2222 },
            new() { NameSystem = "В1", SystemId = 333 }
        };

        for (int i = 1; i <= 7; i++)
        {
            var cloned = new ObservableCollection<EngineeringSystem>(
                systems.Select(s => new EngineeringSystem
                {
                    NameSystem = s.NameSystem,
                    SystemId = s.SystemId
                }));

            list.Add(new Group($"Группа {i}", cloned));
        }

        return list;
    }

    public ObservableCollection<EngineeringSystem> GetProjectSystems()
    {
        return
        [
            new EngineeringSystem { CutSystemName = "П", NameSystem = "П1", SystemId = 1255 },
            new EngineeringSystem { CutSystemName = "П", NameSystem = "П2", SystemId = 1256 },
            new EngineeringSystem { CutSystemName = "В", NameSystem = "В1", SystemId = 20000 },
            new EngineeringSystem { CutSystemName = "В", NameSystem = "В2", SystemId = 20001 }
        ];
    }

    public void AfterClose(ObservableCollection<Group>? groups,  string userParameter) {}
    public List<string> GetUserParameters()
    {
        return ["Параметр 1", "Параметр 2", "пАРАМЕТР 3"];
    }

    public string GetSelectedParameter()
    {
        return "Параметр 1";
    }

    public void Execute(List<Group> systemGroups, string? userParameter, bool createView) => Console.WriteLine("Нажал на Execute");
    
    public void AfterClose() {}
}