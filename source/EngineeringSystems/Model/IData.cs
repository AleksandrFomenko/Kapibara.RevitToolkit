using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;

namespace EngineeringSystems.Model;

public interface IData
{
    public List<EngineeringSystem> GetSystems(string filter);
}