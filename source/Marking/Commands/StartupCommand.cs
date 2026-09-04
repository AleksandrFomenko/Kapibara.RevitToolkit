using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using Marking.Views;
using Nice3point.Revit.Extensions.UI;

namespace Marking.Commands;

/// <summary>
///     External command entry point
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        Host.Start();
        var view = Host.GetService<MarkingView>();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}