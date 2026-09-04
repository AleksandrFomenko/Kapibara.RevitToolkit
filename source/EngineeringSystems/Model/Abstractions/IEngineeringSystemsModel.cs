using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;
using Options = EngineeringSystems.ViewModels.Entities.Options;

namespace EngineeringSystems.Model.Abstractions;

public interface IEngineeringSystemsModel
{
    public void Execute(
        List<string> systemNames,
        string parametersUser,
        bool flag1,
        bool flag2,
        Options options,
        FilterOption filterOption);
    public List<string> GetUserParameters();
}