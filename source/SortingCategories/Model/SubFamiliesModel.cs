using System.Collections.ObjectModel;
using Kapibara.Core;
using SortingCategories.ViewModels;

namespace SortingCategories.Model;

public class SubFamiliesModel(Document document)
{
    public List<string> GetParameters() => document.GetProjectParameterNames().ToList();

    public ObservableCollection<Algorithm> GetAlgorithms ()
    {
        return
        [
            new Algorithm("Значение родительского + 1", IncreaseByOne),
            new Algorithm("Значение родительского + 0.1", IncreaseByOneTenth),
            new Algorithm("Значение родительского + 1 * n", IncreaseByOneMultiply),
            new Algorithm("Значение родительского + 0.1 * n", IncreaseByOneTenthMultiply),
            new Algorithm("Значение родительского", Equate)
        ];
    }

    private List<Element> GetElements()
    {
        return new FilteredElementCollector(document,document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .ToElements()
            .ToList();
    }
    

    private void IncreaseByOne(string parameterSort, string parameterGroup, string groupValue)
    {
        var elements = GetElements();
        using var t = new Transaction(document, "Sorting");
        t.Start();
        foreach (var element in elements)
        {
            var valueStr = string.Empty;

            if (element.TryGetParameterByName(parameterSort, out var pSort)) valueStr = pSort?.AsValueString();
            
            if (!double.TryParse(valueStr, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
                value = 0; 

            foreach (var subElement in element.GetAllSubComponents())
            {
                
                if (subElement.TryGetParameterByName(parameterSort, out var pSubSort))
                {
                    pSubSort?.SetValue(value + 1);
                }
                
                if (subElement.TryGetParameterByName(parameterGroup, out var pSubGroup))
                {
                    pSubGroup?.SetValue(groupValue);
                }
            }
        }
        t.Commit();
    }
    
    private void IncreaseByOneTenth(string parameterSort, string parameterGroup, string groupValue)
    {
        var elements = GetElements();
        using var t = new Transaction(document, "Sorting");
        t.Start();
        foreach (var element in elements)
        {
            var valueStr = string.Empty;

            if (element.TryGetParameterByName(parameterSort, out var pSort)) valueStr = pSort?.AsValueString();
            
            if (!double.TryParse(valueStr, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
                value = 0; 

            foreach (var subElement in element.GetAllSubComponents())
            {
                
                if (subElement.TryGetParameterByName(parameterSort, out var pSubSort))
                {
                    pSubSort?.SetValue(value + 0.1);
                }
                
                if (subElement.TryGetParameterByName(parameterGroup, out var pSubGroup))
                {
                    pSubGroup?.SetValue(groupValue);
                }
            }
        }
        t.Commit();
    }
    
    private void IncreaseByOneMultiply(string parameterSort, string parameterGroup, string groupValue)
    {
        var elements = GetElements();
        using var t = new Transaction(document, "Sorting");
        t.Start();
        foreach (var element in elements)
        {
            var valueStr = string.Empty;

            if (element.TryGetParameterByName(parameterSort, out var pSort)) valueStr = pSort?.AsValueString();
            
            if (!double.TryParse(valueStr, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
                value = 0; 
            
            var count = 1;
            foreach (var subElement in element.GetAllSubComponents())
            {
                
                if (subElement.TryGetParameterByName(parameterSort, out var pSubSort))
                {
                    pSubSort?.SetValue(value + count);
                }
                
                if (subElement.TryGetParameterByName(parameterGroup, out var pSubGroup))
                {
                    pSubGroup?.SetValue(groupValue);
                }
            }
        }
        t.Commit();
    }
    
    private void IncreaseByOneTenthMultiply(string parameterSort, string parameterGroup, string groupValue)
    {
        var elements = GetElements();
        using var t = new Transaction(document, "Sorting");
        t.Start();
        foreach (var element in elements)
        {
            var valueStr = string.Empty;

            if (element.TryGetParameterByName(parameterSort, out var pSort)) valueStr = pSort?.AsValueString();
            
            if (!double.TryParse(valueStr, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
                value = 0; 
            
            var count = 0.1;
            foreach (var subElement in element.GetAllSubComponents())
            {
                
                if (subElement.TryGetParameterByName(parameterSort, out var pSubSort))
                {
                    pSubSort?.SetValue(value + count);
                }
                
                if (subElement.TryGetParameterByName(parameterGroup, out var pSubGroup))
                {
                    pSubGroup?.SetValue(groupValue);
                }
            }
        }
        t.Commit();
    }

    private void Equate(string parameterSort, string parameterGroup, string groupValue)
    {
        
        var elements = GetElements();
        using var t = new Transaction(document, "Sorting");
        t.Start();
        foreach (var element in elements)
        {
            var valueStr = string.Empty;
            
            if (element.TryGetParameterByName(parameterSort, out var pSort))
            {
                valueStr = pSort?.AsValueString();
            }
            
            foreach (var subElement in element.GetAllSubComponents())
            {
                if (subElement.TryGetParameterByName(parameterSort, out var pSortSub))
                {
                    pSortSub?.SetValue(valueStr);
                }
                
                if (subElement.TryGetParameterByName(parameterGroup, out var pSubGroup))
                {
                    pSubGroup?.SetValue(groupValue);
                }
            }
        }
        t.Commit();
    }
}