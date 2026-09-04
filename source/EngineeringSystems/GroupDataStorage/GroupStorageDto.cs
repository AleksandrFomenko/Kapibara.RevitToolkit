using Group = EngineeringSystems.ViewModels.Entities.Group;

namespace EngineeringSystems.GroupDataStorage;

public class GroupStorageDto
{
    public string SelectedUserParameter { get; set; } = string.Empty;
    public List<Group> Data { get; set; } = [];
}