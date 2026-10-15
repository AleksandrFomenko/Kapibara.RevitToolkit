using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Kapibara.Core;
namespace SheetManager.Models;

/// <summary>Invoked from the modal external command, inside the Revit API context.</summary>
public sealed class SheetManagerModel
{
    private readonly Document? _document;
    public bool IsAvailable => _document is { IsValidObject: true, IsFamilyDocument: false, IsReadOnly: false };
    public string DocumentName => _document?.Title ?? "Предпросмотр без документа Revit";
    public SheetManagerModel(Document? document) => _document = document;
    private Document Document => IsAvailable ? _document! :
        throw new InvalidOperationException("Откройте доступный для редактирования проект Revit.");

    public List<TitleBlockItem> GetTitleBlocks()
    {
        if (!IsAvailable) return new();
        return new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsElementType().OfType<FamilySymbol>()
            .Select(s => new TitleBlockItem { Id = s.Id, Name = $"{s.FamilyName} : {s.Name}" })
            .OrderBy(s => s.Name).ToList();
    }
    public List<string> GetParameterNames(TitleBlockItem titleBlock)
    {
        var names = Document.GetProjectParameterNames(BuiltInCategory.OST_Sheets, ParameterBindingKind.Instance)
            .Concat(Document.GetProjectParameterNames(BuiltInCategory.OST_TitleBlocks, ParameterBindingKind.Instance)).ToList();
        var symbol = (FamilySymbol)Document.GetElement(titleBlock.Id);
        if (symbol.Family.IsEditable)
        {
            var familyDocument = Document.EditFamily(symbol.Family);
            try
            {
                names.AddRange(familyDocument.FamilyManager.Parameters.Cast<FamilyParameter>()
                    .Where(p => p.IsInstance && !p.IsReadOnly &&
                        (p.StorageType == StorageType.String || p.StorageType == StorageType.Integer))
                    .Select(p => p.Definition.Name));
            }
            finally { familyDocument.Close(false); }
        }
        return names.Distinct().OrderBy(n => n).ToList();
    }
    public void CreateSheets(TitleBlockItem titleBlock, int count, int startValue,
        string prefix, string suffix, bool systemParameter, bool userParameter, string parameterName)
    {
        if (count <= 0 || startValue < 0 || (long)startValue + count - 1 > int.MaxValue)
            throw new InvalidOperationException("Проверьте количество листов и начальный номер.");
        if (Document.GetElement(titleBlock.Id) is not FamilySymbol)
            throw new InvalidOperationException("Выбранная рамка больше не существует. Откройте окно заново.");
        var numbers = new HashSet<string>(new FilteredElementCollector(Document).OfClass(typeof(ViewSheet))
            .Cast<ViewSheet>().Select(s => s.SheetNumber), StringComparer.OrdinalIgnoreCase);
        using var transaction = new Transaction(Document, "SheetManager: создание листов");
        transaction.Start();
        var options = transaction.GetFailureHandlingOptions();
        options.SetClearAfterRollback(true);
        options.SetFailuresPreprocessor(new CreationFailures());
        transaction.SetFailureHandlingOptions(options);
        for (var i = 0; i < count; i++)
        {
            var value = prefix + (startValue + i).ToString(CultureInfo.InvariantCulture) + suffix;
            var sheet = ViewSheet.Create(Document, titleBlock.Id);
            if (systemParameter)
            {
                // Visible suffixes replace the invisible direction characters in the old module.
                var unique = value;
                var duplicate = 1;
                while (!numbers.Add(unique)) unique = value + "-" + duplicate++;
                sheet.SheetNumber = unique;
            }
            else numbers.Add(sheet.SheetNumber);
            if (userParameter)
            {
                Document.Regenerate();
                var parameter = sheet.LookupParameter(parameterName);
                if (parameter == null)
                {
                    var frame = new FilteredElementCollector(Document, sheet.Id)
                        .OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsNotElementType().FirstOrDefault();
                    parameter = frame?.LookupParameter(parameterName);
                }
                if (parameter == null || parameter.IsReadOnly)
                    throw new InvalidOperationException($"Параметр «{parameterName}» отсутствует или недоступен для записи.");
                var written = parameter.StorageType switch
                {
                    StorageType.String => parameter.Set(value),
                    StorageType.Integer when int.TryParse(value, out var number) => parameter.Set(number),
                    _ => throw new InvalidOperationException($"Параметр «{parameterName}» должен принимать текст или целое число.")
                };
                if (!written) throw new InvalidOperationException($"Не удалось записать параметр «{parameterName}».");
            }
        }
        if (transaction.Commit() != TransactionStatus.Committed)
            throw new InvalidOperationException("Создание листов отменено Revit. Изменения не сохранены.");
    }
    public ObservableCollection<FolderItem> GetSheetOrganization()
    {
        var result = new ObservableCollection<FolderItem>();
        if (!IsAvailable) return result;
        var organization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(Document);
        var sheets = new FilteredElementCollector(Document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>()
            .Where(s => !s.IsPlaceholder && s.CanBePrinted).OrderBy(s => s.SheetNumber);
        foreach (var sheet in sheets)
        {
            var collection = result;
            FolderItem? current = null;
            var folders = organization.GetFolderItems(sheet.Id);
            try
            {
                foreach (var folder in folders)
                {
                    current = collection.FirstOrDefault(f => f.Name == folder.Name);
                    if (current == null)
                    {
                        current = new FolderItem { Name = folder.Name };
                        collection.Add(current);
                    }
                    collection = current.SubFolders;
                }
            }
            finally { foreach (var folder in folders) folder.Dispose(); }
            if (current == null)
            {
                current = result.FirstOrDefault(f => f.Name == "Листы");
                if (current == null) { current = new FolderItem { Name = "Листы" }; result.Add(current); }
            }
            current.Sheets.Add(new SheetItem { Id = sheet.Id, Name = sheet.Name, Number = sheet.SheetNumber });
        }
        return result;
    }
    public void ExportPdf(string directory, IList<ElementId> sheets, bool combine, string fileName)
    {
        if (!Directory.Exists(directory)) throw new InvalidOperationException("Папка для PDF не существует.");
        if (sheets.Count == 0) throw new InvalidOperationException("Выберите хотя бы один лист.");
        using var options = new PDFExportOptions { Combine = combine, StopOnError = true };
        if (combine) options.FileName = fileName;
        if (!Document.Export(directory, sheets, options))
            throw new InvalidOperationException("Revit не завершил экспорт PDF. Часть файлов могла быть сохранена.");
    }
    private sealed class CreationFailures : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor) =>
            accessor.GetFailureMessages().Any(f => f.GetSeverity() == FailureSeverity.Error)
                ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
    }
}
