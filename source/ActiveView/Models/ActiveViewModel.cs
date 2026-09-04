using ActiveView.ViewModels;
using Kapibara.Core;
using Nice3point.Revit.Toolkit.External;


namespace ActiveView.Models;

public partial class ActiveViewModel(Document doc)
{
    public List<string> GetParameters() => doc.GetProjectParameterNames().ToList();

    
    [ExternalEvent]
    private void ExecuteActiveView(string parameterName, string value, bool skipNotEmpty, Option option)
    {
        using var tr = new Transaction(doc, "Kapibara.ActiveView");
        tr.Start();
                
        ICollection<Element> elems;

        if (option.IsAll)
        {
            elems = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements();
        }
        else
        {
            elems = RevitContext.ActiveUiDocument!.Selection
                .GetElementIds()
                .Select(id => doc.GetElement(id))
                .Where(e => e is not null)
                .ToList();
        }

        foreach (var elem in elems)
        {
            if (!elem.TryGetParameterByName(parameterName, out var parameter)) continue;

            if (parameter is null) continue;

            if (skipNotEmpty && CheckParameterValue(parameter))
                continue;

            parameter.SetValue(value);
        }
                
        tr.Commit();
    }


    private static bool CheckParameterValue(Parameter parameter)
    {
        if (parameter is null) return true;
        if (!parameter.HasValue) return false;
    
        return !string.IsNullOrEmpty(parameter.AsValueString());
    }
}
