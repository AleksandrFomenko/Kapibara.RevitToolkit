using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;

namespace RiserMate.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        RiserMateHost.StartServices();
        RiserMateHost.Show();
    }
}