
// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public static class ElementIdExtensions
{
    extension(ElementId elementId)
    {
        public long GetValue()
        {
#if REVIT2024_OR_GREATER
            return elementId.Value;
#else
            return elementId.IntegerValue;
#endif
        }
    }
}