using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;

namespace Axes.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand: ExternalCommand
{
    public override void Execute() => Host.Host.Start();
}