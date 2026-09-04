namespace ClashHub.Extensions;

public static class ElementIdExtensions
{
    public static ElementId ToElementId(this long id)
    {
#if REVIT2024_OR_GREATER
        return new ElementId(id);
#else
        return new ElementId((int)id);
#endif
    }
}