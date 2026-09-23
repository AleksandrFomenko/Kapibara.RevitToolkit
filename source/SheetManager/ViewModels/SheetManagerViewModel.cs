using SheetManager.Models;
namespace SheetManager.ViewModels;

public sealed partial class SheetManagerViewModel : ObservableObject
{
    public string Title => "Sheet Manager";
    public string DocumentName { get; }
    public CreateSheetsViewModel CreateSheets { get; }
    public PrintSheetsViewModel PrintSheets { get; }
    [ObservableProperty] private string _status;
    public SheetManagerViewModel(SheetManagerModel model, CreateSheetsViewModel createSheets, PrintSheetsViewModel printSheets)
    {
        DocumentName = model.DocumentName;
        CreateSheets = createSheets;
        PrintSheets = printSheets;
        _status = model.IsAvailable ? "Создайте листы или выберите листы для печати в PDF."
            : "Откройте проект Revit для создания и печати листов.";
        createSheets.Completed += count =>
        {
            Status = $"Создано листов: {count}.";
            printSheets.RefreshCommand.Execute(null);
        };
        printSheets.Completed += count => Status = $"Экспортировано листов в PDF: {count}.";
    }
}
