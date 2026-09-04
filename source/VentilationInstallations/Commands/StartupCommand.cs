using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using VentilationInstallations.Views;

namespace VentilationInstallations.Commands;

/// <summary>
///     External command entry point.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        Host.Start();
        var view = Host.GetService<VentilationInstallationsView>();
        view.ShowDialog();
    }
}