namespace ViewByParameter.Models;

public class ViewByParameterModelTestUi : IViewByParameterModel
{
    public List<ViewOption?> GetViewOption()
    {
        return
        [
            new ViewOption("3D вид", [
                new Type("3D Первый тип"),
                new Type("3D Второй тип")
            ]),
            new ViewOption("План", [
                new Type("План Первый тип"),
                new Type("План Второй тип")
            ])
        ];
    }

    public List<FilterOption?> GetFilterOption()
    {
        return
        [
            new FilterOption("Равно", "Equals"),
            new FilterOption("Не равно", "NotEquals")
        ];
    }
    
    public List<FilterFromProject?> GetFiltersFromProject()
    {
        return
        [
            new FilterFromProject("Имя 1") { IsEnabled = true, IsVisible = true },
            new FilterFromProject("Имя 2") { IsEnabled = true, IsVisible = false },
            new FilterFromProject("Имя 3") { IsEnabled = false, IsVisible = true },
            new FilterFromProject("Имя 4") { IsEnabled = false, IsVisible = false },
        ];
    }

    public List<string> GetProjectParameters()
    {
        return
        [
            "Параметр1", "Параметр 2", "Параметр 3", "и тд..."
        ];
    }

    public List<ElementsByParameter> GetElementsByParameter(string? parameterName)
    {
        List<ElementsByParameter> result = [];
        for (var i = 0; i < 20; i++)
        {
            result.Add(new ElementsByParameter(i.ToString(), i * 10));
        }

        return result;
    }

    public void Execute(
        List<ElementsByParameter> elementsByParameters, 
        List<FilterFromProject> filtersFromProject, 
        ViewOption viewOption, 
        string parameterName,
        FilterOption filterOption)
    {
        
    }
    
}