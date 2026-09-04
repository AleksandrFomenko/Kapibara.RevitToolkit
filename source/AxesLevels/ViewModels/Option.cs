namespace ProjectAxes.ViewModels;

public class Option(string name, OptionType type)
{
    public string Name { get; set; } = name;
    public OptionType Type { get; set; } = type;
}