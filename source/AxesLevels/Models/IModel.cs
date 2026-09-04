using Nice3point.Revit.Toolkit.External;

namespace AxesLevels.Models;

public interface IModel
{
    Task DoAllAsync(bool beginCheck, bool endCheck);
    Task DoSelectionAsync(bool beginCheck, bool endCheck);
}