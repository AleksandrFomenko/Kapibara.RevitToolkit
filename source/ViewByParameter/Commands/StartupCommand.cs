using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using ViewByParameter.Views;

namespace ViewByParameter.Commands;

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
    }
}