using Nice3point.Revit.Toolkit.External;

namespace AxesLevels.Models;

public class MockModel : IModel
{
    public Task DoAllAsync(bool beginCheck, bool endCheck)
    {
        return Task.CompletedTask;
    }

    public Task DoSelectionAsync(bool beginCheck, bool endCheck)
    {
        return Task.CompletedTask;
    }
}