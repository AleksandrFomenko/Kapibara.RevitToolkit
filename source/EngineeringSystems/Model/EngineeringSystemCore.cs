using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace EngineeringSystems.Model;

public static class EngineeringSystemCore
{
    public static List<Element> GetElementsInSystem(List<string> systemsName, bool sysName)
    {
        if (sysName)
        {
            return systemsName
                .Select(name => GetSystemByName(name))
                .Where(system => system != null)
                .SelectMany(system => GetElementsInSystem(system!) ?? Enumerable.Empty<Element>())
                .ToList();
        }
        return systemsName
            .Select(name => GetSystemByCutName(name))
            .Where(system => system != null)
            .SelectMany(system => GetElementsInSystems(system) ?? Enumerable.Empty<Element>())
            .ToList();
    }
    
    private static List<Element>? GetElementsInSystem(Element mepSystem)
    {
        return mepSystem switch
        {
            PipingSystem pipingSystem => pipingSystem.PipingNetwork.Cast<Element>().ToList(),
            MechanicalSystem ductSystem => ductSystem.DuctNetwork.Cast<Element>().ToList(),
            _ => null
        };
    }
    
    public static List<Element> GetElementsInSystems(List<Element> mepSystem)
    {
        var result = new List<Element>(400); 

        foreach (var sys in mepSystem)
        {
            switch (sys)
            {
                case PipingSystem pipingSystem:
                    result.AddRange(pipingSystem.PipingNetwork.Cast<Element>().Where(e => e != null)); 
                    break;
                
                case MechanicalSystem ductSystem:
                    result.AddRange(ductSystem.DuctNetwork.Cast<Element>().Where(e => e != null));
                    break;
            }
        }
        return result;
    }
    private static  Element? GetSystemByName(string systemName)
    {
        var cats = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_DuctSystem
        };
        
        var catFilter = new ElementMulticategoryFilter(cats);
        
        var mepSystem = new FilteredElementCollector(RevitContext.ActiveDocument)
            .WherePasses(catFilter)
            .WhereElementIsNotElementType()
            .FirstOrDefault(elem => elem.Name == systemName);
        return mepSystem;
    }
    
    private static List<Element> GetSystemByCutName(string name)
    {
        var mepSystemType = GetSystemTypeByCutName(name);
        var cats = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_PipingSystem, BuiltInCategory.OST_Alignments,
            BuiltInCategory.OST_DuctSystem
        };
        var catFilter = new ElementMulticategoryFilter(cats);
        var elementIds = mepSystemType.GetDependentElements(catFilter);
        
        var elements = elementIds
            .Select(id => RevitContext.ActiveDocument!.GetElement(id)) 
            .Where(el => el != null)  
            .ToList();

        return elements;
    }
    
    private static Element GetSystemTypeByCutName(string systemName)
    {
        var cats = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_DuctSystem
        };
        
        var catFilter = new ElementMulticategoryFilter(cats);

        var mepSystemType = new FilteredElementCollector(RevitContext.ActiveDocument)
            .WherePasses(catFilter)
            .WhereElementIsElementType()
            .FirstOrDefault(e =>
                e.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM)?.AsString() == systemName);
        return mepSystemType!;
    }
}