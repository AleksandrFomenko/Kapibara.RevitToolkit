namespace SortingCategories.ViewModels;

public partial class RevitCategory : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private List<Category>? _categories;
    [ObservableProperty] private Category? _category;
    [ObservableProperty] private string? _sorting;
    [ObservableProperty] private string? _group;
}

public partial class Option(string name, bool activeView)
{
    public string Name { get; } = name;
    public bool IsActiveView { get; } = activeView;
}


public class Algorithm(string name, Action<string, string, string> execute)
{
    public string Name { get; } = name;

    public void Execute(string parameterSort, string parameterGroup, string groupValue)
    {
        execute?.Invoke(parameterSort, parameterGroup, groupValue);
    }
}