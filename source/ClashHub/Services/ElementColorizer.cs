namespace ClashHub.Services;

public sealed class ElementColorizer : IElementColorizer
{
    private static readonly Color FirstColor = new(255, 0, 0);
    private static readonly Color SecondColor = new(0, 128, 0);

    public void Colorize(View view, long firstElementId, long secondElementId)
    {
        var solidFillId = GetSolidFillPatternId(view.Document);

        view.SetElementOverrides(firstElementId.ToElementId(), CreateOverrides(FirstColor, solidFillId));
        view.SetElementOverrides(secondElementId.ToElementId(), CreateOverrides(SecondColor, solidFillId));
    }

    private static OverrideGraphicSettings CreateOverrides(Color color, ElementId solidFillId)
    {
        var ogs = new OverrideGraphicSettings()
            .SetProjectionLineColor(color)
            .SetSurfaceForegroundPatternColor(color)
            .SetCutForegroundPatternColor(color);

        if (solidFillId != ElementId.InvalidElementId)
        {
            ogs.SetSurfaceForegroundPatternId(solidFillId);
            ogs.SetCutForegroundPatternId(solidFillId);
        }

        return ogs;
    }

    private static ElementId GetSolidFillPatternId(Document doc)
    {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(FillPatternElement))
            .Cast<FillPatternElement>()
            .FirstOrDefault(fp => fp.GetFillPattern().IsSolidFill)
            ?.Id ?? ElementId.InvalidElementId;
    }
}