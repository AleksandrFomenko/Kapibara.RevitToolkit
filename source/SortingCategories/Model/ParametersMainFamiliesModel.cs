using System.Collections.ObjectModel;
using Kapibara.Core;
using SortingCategories.ViewModels;

namespace SortingCategories.Model;

public class ParametersMainFamiliesModel(Document doc)
{
    public ObservableCollection<RevitCategory> RevitCategories = null!;

    public static List<Option> GetOptions()
    {
        return
        [
            new Option("Все элементы", false),
            new Option("Только на активном виде", true)
        ];
    }

    public List<Category> GetCategory()
    {
        var categories = doc.Settings.Categories;
        return categories
            .Cast<Category>()
            .Where(i => i.IsVisibleInUI)
            .Where(i => i.AllowsBoundParameters)
            .Where(i => i.CategoryType == CategoryType.Model)
            .ToList();
    }

    public List<string> GetParameters() => doc.GetProjectParameterNames().ToList();

    public ObservableCollection<RevitCategory> GetPattern(int i, List<Category> projectCategory)
    {
       return Pattern.GenerateRevitCategories(doc, projectCategory, i);
    }

    public void Execute(string parameterForSort, string parameterForGroup, bool isActiveView, bool checkSubComponents)
    {
        using var t = new Transaction(doc, "Sorting");
        t.Start();
            
        var revitCategories = RevitCategories.Where(cat => cat.IsChecked).ToList();
        foreach (var revitCat in revitCategories)
        {
            var fec = isActiveView
                ? new FilteredElementCollector(doc, doc.ActiveView.Id)
                : new FilteredElementCollector(doc);
            var elems = fec
                .OfCategory((BuiltInCategory)revitCat.Category!.Id.GetValue())
                .WhereElementIsNotElementType()
                .ToElements();
                
            foreach (var elem in elems)
            {
                if (elem is FamilyInstance familyInstance && !checkSubComponents)
                {
                    if (familyInstance.SuperComponent != null) continue;
                }
                if (elem.TryGetParameterByName(parameterForSort, out var parameterSort))
                {
                    parameterSort?.SetValue(revitCat.Sorting);
                }

                if (elem.TryGetParameterByName(parameterForGroup, out var parameterGroup))
                {
                    parameterGroup?.SetValue(revitCat.Group);
                }
            }
        }

        t.Commit();
    }
}