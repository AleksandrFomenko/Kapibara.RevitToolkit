using Autodesk.Revit.Attributes;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;



namespace EngineeringSystems.Commands;

/// <summary>
///     External command entry point invoked from the Revit interface
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommandEngineeringSystems : ExternalCommand
{
    public override void Execute()
    {
        EngineeringSystems.Start();
    }
}

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommandGroupSystems : ExternalCommand
{
    public override void Execute()
    {
        GroupSystems.Start();
    }
}