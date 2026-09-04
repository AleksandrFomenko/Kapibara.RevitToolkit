using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using LegendPlacer.Services;

namespace LegendPlacer.Models
{
    public class LegendPlacerModel(
        Document document,
        SheetOrganizationService sheetOrganizationService)
        : ILegendPlacerModel
    {
        private readonly Document _doc = document 
                                         ?? throw new ArgumentNullException(nameof(document));
        private readonly SheetOrganizationService _sheetOrganizationService = sheetOrganizationService
                                                                              ?? throw new ArgumentNullException(nameof(sheetOrganizationService));

        public List<string>? GetLegends(string? legendName)
        {
            var filter = legendName ?? string.Empty;

            return new FilteredElementCollector(_doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.ViewType == ViewType.Legend
                            && v.Name
                                .IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(v => v.Name)
                .ToList();
        }

        private View? GetProjectLegendByName(string legendName)
        {
            if (string.IsNullOrWhiteSpace(legendName))
                return null;
            
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(v =>
                    v.ViewType == ViewType.Legend &&
                    v.Name.IndexOf(legendName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<string?> GetCorners()
            =>
            [
                "Левый нижний",
                "Левый верхний",
                "Правый нижний",
                "Правый верхний"
            ];

        public ObservableCollection<FolderItem> GetSheetItem()
            => _sheetOrganizationService.GetSheetOrganization();
        
        private static IEnumerable<SheetItem> GetCheckedSheetsRecursively(FolderItem folder)
        {
            foreach (var sheet in folder.Sheets.Where(s => s.IsChecked))
                yield return sheet;
            
            foreach (var sub in folder.SubFolders)
            foreach (var child in GetCheckedSheetsRecursively(sub))
                yield return child;
        }

        public void Execute(
            IEnumerable<FolderItem> folders, 
            string legendName, 
            string? position, 
            double xChange, 
            double yChange)
        {
            var legend = GetProjectLegendByName(legendName);
            if (legend == null) return;
            
            var checkedSheetIds = folders
                .SelectMany(GetCheckedSheetsRecursively)
                .Select(si => new ElementId(si.ElemId))
                .ToList();
            using (var t = new Transaction(_doc,"Legend placer"))
            {
                t.Start();
                foreach (var sheetId in checkedSheetIds)
                {
                    if (_doc.GetElement(sheetId) is not View sheetView) continue;

                    var outline = sheetView.Outline;
                    double x = 0, y = 0;

                    switch (position)
                    {
                        case "Правый нижний":
                            x = outline.Max.U - xChange;
                            y = outline.Min.V + yChange;
                            break;
                        case "Правый верхний":
                            x = outline.Max.U - xChange;
                            y = outline.Max.V - yChange;
                            break;
                        case "Левый нижний":
                            x = outline.Min.U + xChange;
                            y = outline.Min.V + yChange;
                            break;
                        case "Левый верхний":
                            x = outline.Min.U + xChange;
                            y = outline.Max.V - yChange;
                            break;
                    }

                    var placement = new XYZ(x, y, 0); 
                    Viewport.Create(_doc, sheetView.Id, legend?.Id, placement);
                }
                t.Commit();
            }
        }
    }
}