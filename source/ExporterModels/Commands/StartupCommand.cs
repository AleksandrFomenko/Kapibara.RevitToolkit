using Autodesk.Revit.Attributes;
using ExporterModels.services;
using Nice3point.Revit.Toolkit.External;

namespace ExporterModels.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        Handlers.RegisterHandlers();
        Host.Start();
    }
}