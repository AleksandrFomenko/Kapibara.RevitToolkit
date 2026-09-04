using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

[Flags]
public enum ParameterBindingKind
{
    Instance = 1,
    Type = 2,
    Any = Instance | Type
}

public static class ParameterExtensions
{
    private const double DoubleTolerance = 1e-9;
    
    /// <summary>
    /// Расширения для работы с параметрами в модели.
    /// ParameterBindingKind.Instance - вернет параметры экземпляра,
    /// ParameterBindingKind.Type - вернет параметры типа,
    /// ParameterBindingKind.Any - вернет параметры и типа и экземпляра, данный вариант ПО УМОЛЧАНИЮ.
    /// </summary>
    extension(Document document)
    {
        public IReadOnlyList<string> GetProjectParameterNames(
            ParameterBindingKind bindingKind = ParameterBindingKind.Any)
            => CollectNames(document,
                (_, binding) => MatchesBindingKind(binding, bindingKind));

        public IReadOnlyList<string> GetProjectParameterNames(
            BuiltInCategory category,
            ParameterBindingKind bindingKind = ParameterBindingKind.Any)
            => CollectNames(document,
                (_, binding) => MatchesBindingKind(binding, bindingKind)
                                && HasCategory(binding, category));
        
        public IReadOnlyList<string> GetProjectParameterNames(
            ForgeTypeId dataType,
            ParameterBindingKind bindingKind = ParameterBindingKind.Any)
            => CollectNames(document, (def, b) => MatchesBindingKind(b, bindingKind) && MatchesDataType(def, dataType));
        
        public Definition? GetProjectParameterDefinition(string parameterName)
        {
            var bindingMap = document.ParameterBindings;
            if (bindingMap == null) return null;

            var iterator = bindingMap.ForwardIterator();
            while (iterator.MoveNext())
                if (iterator.Key is { } definition)
                    if (definition.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                        return definition;

            return null;
        }
        
        public List<BuiltInCategory>? GetCategoriesByParameter(string parameterName)
        {
            var bindingMap = document.ParameterBindings;
            var iterator = bindingMap.ForwardIterator();
            iterator.Reset();

            while (iterator.MoveNext())
            {
                var definition = iterator.Key;
                var binding = iterator.Current;

                if (definition == null || definition.Name != parameterName)
                    continue;

                CategorySet? categories = null;

                if (binding is InstanceBinding instanceBinding)
                    categories = instanceBinding.Categories;
                else if (binding is TypeBinding typeBinding)
                    categories = typeBinding.Categories;

                if (categories == null)
                    continue;

                var builtInCategories = new List<BuiltInCategory>();
                var catIterator = categories.ForwardIterator();
                catIterator.Reset();

                while (catIterator.MoveNext())
                {
                    var category = catIterator.Current as Category;
                    if (category == null)
                        continue;

#if REVIT2023_OR_GREATER
                    try
                    {
                        builtInCategories.Add(category.BuiltInCategory);
                    }
                    catch
                    {
                        // ingored
                    }
#else
            var id = category.Id.GetIntValue();
            var underlyingType = Enum.GetUnderlyingType(typeof(BuiltInCategory));
            var boxed = underlyingType == typeof(long) ? (object)id : (object)(int)id;

            if (Enum.IsDefined(typeof(BuiltInCategory), boxed))
                builtInCategories.Add((BuiltInCategory)boxed);
#endif
                }

                return builtInCategories;
            }
            return null;
        }
    }
    
    private static bool MatchesBindingKind(ElementBinding binding, ParameterBindingKind kind)
        => binding switch
        {
            InstanceBinding => kind.HasFlag(ParameterBindingKind.Instance),
            TypeBinding     => kind.HasFlag(ParameterBindingKind.Type),
            _ => false
        };

    private static IReadOnlyList<string> CollectNames(
        Document document,
        Func<Definition, ElementBinding, bool> predicate)
    {
        var result = new List<string>();
        var iterator = document.ParameterBindings.ForwardIterator();

        while (iterator.MoveNext())
        {
            if (iterator.Key is not { } definition) continue;
            if (iterator.Current is not ElementBinding binding) continue;
            if (!predicate(definition, binding)) continue;

            result.Add(definition.Name);
        }

        result.Sort();
        return result;
    }

    private static bool HasCategory(ElementBinding binding, BuiltInCategory target)
    {
        var categories = binding.Categories;
        if (categories is null) return false;

        return categories.Cast<Category>().Any(c =>
#if REVIT2023_OR_GREATER
                c.BuiltInCategory == target
#else
                (BuiltInCategory)c.Id.IntegerValue == target
#endif
        );
    }

    private static bool MatchesDataType(Definition definition, ForgeTypeId type) => definition.GetDataType() == type;

    /// <summary>
    /// Расширения для работы с элементом
    /// </summary>
    extension(Element element)
    {
        public IReadOnlyList<Parameter> GetParametersCore()
            => element.Parameters.Cast<Parameter>().ToList();

        IReadOnlyList<string> GetParameterNames()
        {
            var names = new List<string>();
            foreach (Parameter p in element.Parameters)
            {
                if (p.Definition is { } def)
                    names.Add(def.Name);
            }
            names.Sort();
            return names;
        }

        private IReadOnlyList<string> GetParameterNames(ParameterBindingKind kind = ParameterBindingKind.Any)
        {
            var names = new HashSet<string>();

            if (kind.HasFlag(ParameterBindingKind.Instance))
            {
                foreach (Parameter p in element.Parameters)
                    if (p.Definition is { } def)
                        names.Add(def.Name);
            }

            if (kind.HasFlag(ParameterBindingKind.Type))
            {
                var typeId = element.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    var type = element.Document.GetElement(typeId);
                    if (type is not null)
                    {
                        foreach (Parameter p in type.Parameters)
                            if (p.Definition is { } def)
                                names.Add(def.Name);
                    }
                }
            }

            var result = names.ToList();
            result.Sort();
            return result;
        }
        /*
        Принимается ТОЛЬКО такое использование!!!!
         
        if (element.TryGetParameterByName("мойПараметр", out var параметр))
        {
            var value = comments?.AsString();
        }
        */

        public bool TryGetParameterByName(string parameterName, out Parameter? parameter)
        {
            parameter = element.LookupParameter(parameterName);
            if (parameter is not null) return true;

            var typeId = element.GetTypeId();
            if (typeId == ElementId.InvalidElementId) return false;

            var elementType = element.Document.GetElement(typeId);
            parameter = elementType?.LookupParameter(parameterName);
            return parameter is not null;
        }
    }


    /// <summary>
    /// Расширения для работы с параметрами
    /// </summary>
    extension(Parameter parameter)
    {
        public bool SetValue(object? value)
        {
            if (parameter is null || parameter.IsReadOnly) return false;

            return parameter.StorageType switch
            {
                StorageType.Integer   => TrySetInteger(parameter, value),
                StorageType.Double    => TrySetDouble(parameter, value),
                StorageType.String    => TrySetString(parameter, value),
                StorageType.ElementId => TrySetElementId(parameter, value),
                _ => false
            };
        }
    }
    

    private static bool TrySetInteger(Parameter parameter, object? value)
    {
        if (!TryGetInteger(value, out var intValue))
            return false;

        if (parameter.AsInteger() == intValue)
            return true; 

        return parameter.Set(intValue);
    }

    private static bool TryGetInteger(object? value, out int result)
    {
        switch (value)
        {
            case int i:
                result = i; return true;
            case bool b:
                result = b ? 1 : 0; return true;
            case string s when TryParseBoolean(s, out var boolValue):
                result = boolValue ? 1 : 0; return true;
        }

        return int.TryParse(value?.ToString(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out result);
    }
    
    private static bool TrySetDouble(Parameter parameter, object? value)
    {
        if (!TryGetDouble(value, out var raw))
            return false;

        var internalValue = ConvertToInternalUnits(parameter, raw);

        if (Math.Abs(parameter.AsDouble() - internalValue) < DoubleTolerance)
            return true;

        return parameter.Set(internalValue);
    }

    private static bool TryGetDouble(object? value, out double result)
    {
        switch (value)
        {
            case double d: result = d; return true;
            case float f:  result = f; return true;
            case int i:    result = i; return true;
            default:
                return double.TryParse(value?.ToString(),
                                       NumberStyles.Any,
                                       CultureInfo.InvariantCulture,
                                       out result);
        }
    }

    private static double ConvertToInternalUnits(Parameter parameter, double value)
    {
        try
        {
#if REVIT2022_OR_GREATER
            var unitTypeId = parameter.GetUnitTypeId();
            return UnitUtils.ConvertToInternalUnits(value, unitTypeId);
#else
            var displayUnit = parameter.DisplayUnitType;
            return UnitUtils.ConvertToInternalUnits(value, displayUnit);
#endif
        }
        catch
        {
            return value;
        }
    }
    
    private static bool TrySetString(Parameter parameter, object? value)
    {
        var str = value?.ToString() ?? string.Empty;
        if (parameter.AsString() == str) return true;
        return parameter.Set(str);
    }
    
    private static bool TrySetElementId(Parameter parameter, object? value)
    {
        if (value is not ElementId id) return false;
        if (parameter.AsElementId() == id) return true;
        return parameter.Set(id);
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "да" or "yes" or "true" or "1":
                result = true; return true;
            case "нет" or "no" or "false" or "0":
                result = false; return true;
            default:
                result = false; return false;
        }
    }
    
}