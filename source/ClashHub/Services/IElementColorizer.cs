using Autodesk.Revit.DB;

namespace ClashHub.Services;

public interface IElementColorizer
{
    void Colorize(View view, long firstElementId, long secondElementId);
}