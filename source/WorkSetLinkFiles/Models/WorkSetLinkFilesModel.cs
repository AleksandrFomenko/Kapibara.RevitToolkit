
namespace WorkSetLinkFiles.Models;

internal class WorkSetLinkFilesModel
{
    private readonly Data _data;
    private static Document _doc = null!;
    
    internal WorkSetLinkFilesModel(Document doc)
    {
        _doc = doc;
        _data = new Data(_doc);
    }

    internal void SetLinksWorkset(List<LinkFiles> linkFilesList, string? suffix, string? prefix)
    {
        foreach (var link in linkFilesList)
        {
            var workset = Workset.Create(_doc, prefix + link.WorksetName + suffix);
            var linkModel = _data.GetLink(link.RevitModelName);

            linkModel?.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)?.Set(workset.Id.IntegerValue);
            _doc.GetElement(linkModel?.GetTypeId())?.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)?.Set(workset.Id.IntegerValue);
        }
    }
    
    internal void SetWorkset(string worksetName, BuiltInCategory cat)
    {
        if (worksetName ==string.Empty) return;
        
        var workset = new FilteredWorksetCollector(_doc)
            .OfKind(WorksetKind.UserWorkset)
            .ToWorksets()
            .FirstOrDefault(w => w.Name == worksetName);
        
        new FilteredWorksetCollector(_doc)
            .OfKind(WorksetKind.UserWorkset)
            .ToWorksets()
            .Select(w => w.Name)
            .ToList();
        var elems = new FilteredElementCollector(_doc)
            .OfCategory(cat)
            .WhereElementIsNotElementType()
            .ToList();
        if (workset?.Id?.IntegerValue == null) return;
        var worksetId = (int)workset?.Id.IntegerValue!;
        elems.ForEach(a =>
        {
            var parameter = a?.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
            parameter?.Set(worksetId);
        });
    }
}