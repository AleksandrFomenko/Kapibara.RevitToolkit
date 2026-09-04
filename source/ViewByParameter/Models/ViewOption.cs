namespace ViewByParameter.Models;

public class Type(string name)
{
    public string Name { get; set; } = name;
}
public class ViewOption(string name, List<Type> types)
{
    public string Name { get; set; } = name;
    public List<Type> Types { get; set; } = types;
    public Type? Type { get; set; } = types.FirstOrDefault();
}
