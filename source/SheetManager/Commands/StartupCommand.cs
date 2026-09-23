using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using SheetManager.Host;

namespace SheetManager.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        var document = Application.ActiveUIDocument?.Document;
        if (document == null || document.IsFamilyDocument || document.IsReadOnly)
        {
            TaskDialog.Show("Sheet Manager", "Откройте доступный для редактирования проект Revit.");
            return;
        }
        SheetManagerHost.Build(document);
        SheetManagerHost.Run(Application.MainWindowHandle);
    }
}
