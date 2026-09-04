namespace WorkSetLinkFiles.Models;

internal class Data
{
    private readonly Document _doc;

    internal Data(Document doc)
    {
        _doc = doc;
    }
    internal List<LinkFiles> GetTestLinks()
    {
        return
        [
            new LinkFiles()
            {
                IsChecked = true,
                RevitModelName = "Somethink",
                WorksetName = "Workset"
            },

            new LinkFiles()
            {
                IsChecked = false,
                RevitModelName = "Somethink2",
                WorksetName = "Workset2"
            }
        ];
    }
    internal List<LinkFiles> GetLinks()
    {
        return new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .ToElements()
            .Select(element =>
            {
                var typeElement = _doc.GetElement(element.GetTypeId()) as ElementType;
                var name = typeElement?.Name ?? "error";
                if (name.EndsWith("rvt", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - 4);
                }
                return new LinkFiles
                {
                    IsChecked = true,
                    RevitModelName = typeElement?.Name ?? "error", 
                    WorksetName = name,
                    Prefix = string.Empty,
                    Suffix = string.Empty
                };
            })
            .ToList();
    }

    internal Element GetLink(string? name)
    {
        return new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .FirstOrDefault(element =>
            {
                var typeElement = _doc.GetElement(element.GetTypeId()) as ElementType;
                return typeElement != null &&
                       typeElement.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            });
    }

    internal List<string> GetWorksets()
    {
        return new FilteredWorksetCollector(_doc)
            .OfKind(WorksetKind.UserWorkset)
            .ToWorksets()
            .Select(w => w.Name)
            .ToList();
    }
}