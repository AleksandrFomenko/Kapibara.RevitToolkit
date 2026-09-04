using System.Text;
using System.Globalization;

namespace VentilationInstallations.Model;

public static class FormulaEvaluator
{
    private abstract record Token;
    private sealed record TextToken(string Value) : Token;
    private sealed record ParamToken(string Name) : Token;

    public static string Evaluate(string formula, Element element)
    {
        var sb = new StringBuilder();

        foreach (var token in Parse(formula))
        {
            switch (token)
            {
                case TextToken text:
                    sb.Append(text.Value);
                    break;
                case ParamToken paramToken:
                    sb.Append(ResolveParameterValue(element, paramToken.Name));
                    break;
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<Token> Parse(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula)) yield break;

        var i = 0;
        while (i < formula.Length)
        {
            var c = formula[i];

            if (c == '\'')
            {
                i++;
                var sb = new StringBuilder();
                var closed = false;

                while (i < formula.Length)
                {
                    if (formula[i] == '\'')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '\'')
                        {
                            sb.Append('\'');
                            i += 2;
                            continue;
                        }
                        closed = true;
                        i++;
                        break;
                    }
                    sb.Append(formula[i++]);
                }

                if (!closed)
                    throw new FormatException($"Незакрытая кавычка в формуле: {formula}");

                if (sb.Length > 0)
                    yield return new TextToken(sb.ToString());
                continue;
            }

            if (c == '[')
            {
                var end = formula.IndexOf(']', i + 1);
                if (end < 0)
                    throw new FormatException($"Незакрытая скобка в формуле: {formula}");

                var name = formula[(i + 1)..end].Trim();
                if (name.Length == 0)
                    throw new FormatException($"Пустое имя параметра в формуле: {formula}");

                yield return new ParamToken(name);
                i = end + 1;
                continue;
            }
            
            var start = i;
            while (i < formula.Length && formula[i] != '\'' && formula[i] != '[')
                i++;

            var bare = formula[start..i];
            if (!string.IsNullOrWhiteSpace(bare))
                yield return new TextToken(bare);
        }
    }
    

    private static string ResolveParameterValue(Element element, string name)
    {
        var parameter = FindParameter(element, name);
        return parameter is null ? string.Empty : ReadAsText(parameter);
    }


    private static Parameter? FindParameter(Element element, string name)
    {
        var parameter = element.LookupParameter(name);
        if (parameter is not null) return parameter;

        var typeId = element.GetTypeId();
        if (typeId == ElementId.InvalidElementId) return null;

        return element.Document.GetElement(typeId)?.LookupParameter(name);
    }

    private static string ReadAsText(Parameter parameter)
    {
        if (!parameter.HasValue) return string.Empty;

        return parameter.StorageType switch
        {
            StorageType.String    => parameter.AsString() ?? string.Empty,
            StorageType.Integer   => parameter.AsInteger().ToString(CultureInfo.InvariantCulture),
            StorageType.Double    => FormatDouble(parameter),
            StorageType.ElementId => ReadElementIdName(parameter),
            _                     => string.Empty
        };
    }

    private static string FormatDouble(Parameter parameter)
    {
        var value = UnitUtils.ConvertFromInternalUnits(
            parameter.AsDouble(),
            parameter.GetUnitTypeId());

        return Math.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string ReadElementIdName(Parameter parameter)
    {
        var id = parameter.AsElementId();
        if (id == ElementId.InvalidElementId) return string.Empty;

        return parameter.Element.Document.GetElement(id)?.Name ?? string.Empty;
    }
}