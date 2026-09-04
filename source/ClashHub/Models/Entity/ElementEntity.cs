namespace ClashHub.Models.Entity;

public class ElementEntity(string type, long id, string familyType)
{
    public string Type { get; } = type;
    public long Id { get; } = id;
    public string FamilyType { get; } = familyType;
}