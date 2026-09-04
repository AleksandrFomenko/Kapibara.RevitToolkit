using System.Diagnostics;
using Autodesk.Revit.UI;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ImportExcelByParameter.ViewModels;

namespace ImportExcelByParameter.Models.excel;

internal class ExcelWorker
{
    private int _columnParameterIndex = 0;
    private XLWorkbook? _workbook;
    private IXLWorksheet? _worksheet;
    
    internal string? SheetName;
    internal string? ParameterName;
    internal int? RowNumber;
    
    internal static Action? CloseExcel { get; set; }
    
    internal void OpenExcel(string path)
    {
        try
        {
            _workbook = new XLWorkbook(path);
            _worksheet = _workbook.Worksheet(SheetName);
            CloseExcel = () => _workbook?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }
    internal List<string> GetWorksheetNames(string path)
    {
        if (string.IsNullOrEmpty(path)) return [];
        try
        {
            _workbook = new XLWorkbook(path);
            return _workbook?.Worksheets.Select(ws => ws.Name).ToList()!;
        }
        catch (Exception ex) when (ex.HResult == -2147024864)
        {
            TaskDialog.Show("Err", "ексель закрой");
            return [];
        }
        finally
        {
            _workbook?.Dispose();
        }
    }
    private List<int> GetParameterColumn()
    {
        var columnIndex = 1;
        var emptyCellCount = 0;
        const int maxEmptyCells = 3;
        var lastColumn = _worksheet!.LastColumnUsed()?.ColumnNumber() ?? 0;

        var otherColumns = new List<int>();

        while (columnIndex <= lastColumn)
        {
            var cell = _worksheet.Cell((int)RowNumber!, columnIndex);
            var cellValue = cell.GetString().Trim();

            if (string.Equals(cellValue, ParameterName, StringComparison.OrdinalIgnoreCase))
            {
                _columnParameterIndex = columnIndex;
                columnIndex++;
                continue; 
            }

            if (string.IsNullOrEmpty(cellValue))
            {
                emptyCellCount++;
                if (emptyCellCount >= maxEmptyCells)
                {
                    break;
                }
            }
            else
            {
                emptyCellCount = 0;
                otherColumns.Add(columnIndex);
            }

            columnIndex++;
        }

        return (otherColumns);
    }
    
    private int FindRow(string searchValue)
    {
        var rowIndex = RowNumber + 1; 
        var emptyCount = 0;
        const int stop = 10;
        var maxRows = 1048576;

        while (rowIndex <= maxRows)
        {
            var cell =_worksheet!.Cell((int)rowIndex, _columnParameterIndex);

            if (cell.IsEmpty())
            {
                emptyCount++;
                if (emptyCount >= stop)
                {
                    break;
                }
            }
            else
            {
                emptyCount = 0; 
                var cellValue = cell.GetString().Trim();
                
                if (string.Equals(cellValue, searchValue, StringComparison.OrdinalIgnoreCase))
                {
                    return (int)rowIndex;
                }
            }
            rowIndex++;
        }
        return 0;
    }

    internal Dictionary<string, string> Execute(string searchValue)
    {
        var otherColumns = GetParameterColumn();
        if (_columnParameterIndex == 0)
        {
            TaskDialog.Show("Error", $"Параметр {ParameterName} не найден в строчке {RowNumber}");
            ImportExcelByParameterViewModel.CloseWindow?.Invoke();
        }
        
        var row = FindRow(searchValue);

        if (row <= 0) return new Dictionary<string, string>();
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
        var paramColumnName = _worksheet?.Cell((int)RowNumber!, _columnParameterIndex).GetString();
        var paramValue = _worksheet?.Cell(row, _columnParameterIndex).GetString();
        if (paramColumnName != null)
            if (paramValue != null)
                result[paramColumnName] = paramValue;

        foreach(var col in otherColumns)
        {
            var colName = _worksheet?.Cell((int)RowNumber!, col).GetString();
            var cellValue = _worksheet?.Cell(row, col).GetString();
            cellValue = cellValue?.Replace("\n", Environment.NewLine);
            if (colName == null) continue;
            if (cellValue != null)
                result[colName] = cellValue;
        }
        return result;
    }
}
