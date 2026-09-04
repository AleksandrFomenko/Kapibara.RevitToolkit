using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;


namespace WorkSetLinkFiles.Commands;

[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        Host.Start();
    }
}