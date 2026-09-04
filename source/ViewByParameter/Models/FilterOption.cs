namespace ViewByParameter.Models;

public class FilterOption(string name, string revitApiMethodName)
{
    public string Name { get; set; } = name;
    public string RevitApiMethodName { get; set; } = revitApiMethodName;
}