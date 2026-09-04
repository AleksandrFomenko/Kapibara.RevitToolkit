using System.Collections.ObjectModel;
using Kapibara.Core;


namespace SortingCategories.ViewModels;

public static class Pattern
{
    
    private static Dictionary<BuiltInCategory, int> DuctsNumber { get; } = new Dictionary<BuiltInCategory, int>
    {
        { BuiltInCategory.OST_MechanicalEquipment, 100 },
        { BuiltInCategory.OST_DuctCurves, 200 },
        { BuiltInCategory.OST_DuctFitting, 300 },
        { BuiltInCategory.OST_DuctAccessory, 400 },
        { BuiltInCategory.OST_DuctTerminal, 500 },
        { BuiltInCategory.OST_DuctInsulations, 600 },
        { BuiltInCategory.OST_DuctLinings, 600 },
        { BuiltInCategory.OST_GenericModel, 800 }
    };
    
    private static readonly Dictionary<BuiltInCategory, string> DuctsGroup = new Dictionary<BuiltInCategory, string>
    {
        { BuiltInCategory.OST_DuctCurves, "Воздуховоды" },
        { BuiltInCategory.OST_MechanicalEquipment, "Оборудование" },
        { BuiltInCategory.OST_DuctFitting, "Фасонные детали воздуховодов" },
        { BuiltInCategory.OST_DuctAccessory, "Арматура воздуховодов" },
        { BuiltInCategory.OST_DuctInsulations, "Материалы изоляции воздуховодов" },
        { BuiltInCategory.OST_DuctLinings, "Материалы изоляции воздуховодов" },
        { BuiltInCategory.OST_DuctTerminal, "Воздухораспределительные устройства" },
        { BuiltInCategory.OST_GenericModel, "Прочие элементы" }
    };

    private static Dictionary<BuiltInCategory, int> PipeNumber { get; } = new Dictionary<BuiltInCategory, int>
    {
        { BuiltInCategory.OST_MechanicalEquipment, 100 }, 
        { BuiltInCategory.OST_PipeCurves, 200 },
        { BuiltInCategory.OST_PipeFitting, 300 },       
        { BuiltInCategory.OST_PipeAccessory, 400 },     
        { BuiltInCategory.OST_PipeInsulations, 500 },
        { BuiltInCategory.OST_GenericModel, 600 }
    };

    private static readonly Dictionary<BuiltInCategory, string> PipeGroup = new Dictionary<BuiltInCategory, string>
    {
        { BuiltInCategory.OST_MechanicalEquipment, "Отопительные приборы" },
        { BuiltInCategory.OST_PipeCurves, "Трубопроводы" },
        { BuiltInCategory.OST_PipeFitting, "Фитинги трубопроводов" },
        { BuiltInCategory.OST_PipeAccessory, "Арматура трубопроводов" },
        { BuiltInCategory.OST_PipeInsulations, "Изоляция" },
        { BuiltInCategory.OST_GenericModel, "Прочие элементы" }
    };
    
    private static Dictionary<BuiltInCategory, int> PipeNumberWater { get; } = new Dictionary<BuiltInCategory, int>
    {
        { BuiltInCategory.OST_MechanicalEquipment, 100 }, 
        { BuiltInCategory.OST_PipeCurves, 200 },
        { BuiltInCategory.OST_PipeFitting, 300 },       
        { BuiltInCategory.OST_PipeAccessory, 400 },     
        { BuiltInCategory.OST_PipeInsulations, 500 },
        { BuiltInCategory.OST_PlumbingFixtures, 600},
        { BuiltInCategory.OST_GenericModel, 700 }
    };

    private static readonly Dictionary<BuiltInCategory, string> PipeGroupWater = new Dictionary<BuiltInCategory, string>
    {
        { BuiltInCategory.OST_MechanicalEquipment, "Оборудование" },
        { BuiltInCategory.OST_PipeCurves, "Трубопроводы" },
        { BuiltInCategory.OST_PipeFitting, "Фитинги трубопроводов" },
        { BuiltInCategory.OST_PipeAccessory, "Арматура трубопроводов" },
        { BuiltInCategory.OST_PipeInsulations, "Изоляция" },
        { BuiltInCategory.OST_PlumbingFixtures, "Сантехнические приборы" },
        { BuiltInCategory.OST_GenericModel, "Прочие элементы" }
    };
    
    
    public static ObservableCollection<RevitCategory> GenerateRevitCategories(Document doc,
        List<Category> cats, int systemType)
    {
        if (doc == null) 
            throw new ArgumentNullException(nameof(doc));
    
        var result = new ObservableCollection<RevitCategory>();
        Dictionary<BuiltInCategory, int> numberMap;
        Dictionary<BuiltInCategory, string> groupMap;
    
        switch (systemType)
        {
            case 1: 
                numberMap = DuctsNumber;
                groupMap = DuctsGroup;
                break;
            case 2: 
                numberMap = PipeNumber;
                groupMap = PipeGroup;
                break;
            case 3: 
                numberMap = PipeNumberWater;
                groupMap = PipeGroupWater;
                break;
            default:
                numberMap = new Dictionary<BuiltInCategory, int> 
                { 
                    { BuiltInCategory.OST_GenericModel, 900 } 
                };
                groupMap = new Dictionary<BuiltInCategory, string>
                {
                    { BuiltInCategory.OST_GenericModel, "Обобщенные модели" }
                };
                break;
        }

        foreach (var builtInCategory in numberMap.Keys)
        {
            var category = cats.FirstOrDefault(c => c.Id.GetValue() == (int)builtInCategory);
    
            if (category == null) 
                continue;
            
            if (!groupMap.TryGetValue(builtInCategory, out var groupName))
            {
                groupName = "Прочие элементы";
            }
            
            var sortingValue = numberMap[builtInCategory];

            result.Add(new RevitCategory
            {
                IsChecked = true,
                Categories = cats,
                Category = category,
                Sorting = sortingValue.ToString(),
                Group = groupName
            });
        }
    
        return result;
    }
    
}