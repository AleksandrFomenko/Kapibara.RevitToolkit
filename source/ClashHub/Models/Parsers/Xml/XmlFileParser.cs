#nullable enable
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ClashHub.Domain.Entities;
using ClashHub.Structure;
using ClashObject = ClashHub.Domain.Entities.ClashObject;
using ClashResult = ClashHub.Domain.Entities.ClashResult;
using ObjectAttribute = ClashHub.Domain.Entities.ObjectAttribute;
using SmartTag = ClashHub.Domain.Entities.SmartTag;

namespace ClashHub.Models.Parsers.Xml;

public class XmlFileParser : IFileParser<ClashTest>
{
    private XmlStructure? _xmlStructure; 
    public string FormatName => "XML";
    private string? _filePath;
    private readonly ChangeTracker _tracker = new();
    private readonly Dictionary<Guid, Structure.ClashResult> _xmlResultMap = new();
    
    public IEnumerable<ClashTest> Parse(string filePath)
    {
        _filePath = filePath;
        using var reader = XmlReader.Create(filePath);
        var serializer = new XmlSerializer(typeof(XmlStructure));
        _xmlStructure = (XmlStructure)serializer.Deserialize(reader)!;
        _tracker.StatusChanged += OnStatusChanged;
        return _xmlStructure.Batchtest?.Clashtests?.ClashTestList?
                   .Select(ConvertTest) 
               ?? [];
    }
    
    private void OnStatusChanged(Guid id, string newStatus)
    {
        if (_xmlResultMap.TryGetValue(id, out var xmlResult))
        {
            xmlResult.Status = newStatus;
            SaveChanges();
        }
    }
    
    private void SaveChanges()
    {
        if (_xmlStructure == null || string.IsNullOrEmpty(_filePath)) return;

        try
        {
            var serializer = new XmlSerializer(typeof(XmlStructure));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            
            using var writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, _xmlStructure, namespaces);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
        }
    }
    
    private ClashTest ConvertTest(Clashtest xmlTest)
    {
        var summary = ConvertSummary(xmlTest.Summary);
        var results = xmlTest.ClashResults?.Results?
            .Select(ConvertClashResult)
            .ToList() ?? [];
        var test = new ClashTest(xmlTest.Name!, xmlTest.TestType!, summary!);
        
        foreach (var result in results)
        {
            test.Results.Add(result);
            
            result.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(ClashResult.Status)) return;
                test.RecalculateSummary();
                _tracker.NotifyStatusChanged(result.Id, result.Status);
            };
        }
    
       
        test.RecalculateSummary();
        return test;
    }

    private ClashSummary? ConvertSummary(Summary? xmlSummary)
    {
        return new ClashSummary(
            total: ParseInt(xmlSummary?.Total),
            @new: ParseInt(xmlSummary?.New),
            active: ParseInt(xmlSummary?.Active),
            reviewed: ParseInt(xmlSummary?.Reviewed),
            approved: ParseInt(xmlSummary?.Approved),
            resolved: ParseInt(xmlSummary?.Resolved),
            testType: xmlSummary?.TestType!
        );
    }

    private ClashResult ConvertClashResult(Structure.ClashResult xmlResult)
    {
        var guid = ParseGuid(xmlResult.Guid);
        var point = new Point3D(
            xmlResult.ClashPoint?.Pos?.X ?? 0,  
            xmlResult.ClashPoint?.Pos?.Y ?? 0,
            xmlResult.ClashPoint?.Pos?.Z ?? 0
        );
        
        var result = new ClashResult(
            id: guid,
            name: xmlResult.Name!,
            status: xmlResult.Status,
            distance: xmlResult.Distance,
            created: xmlResult.Created,
            clashPoint: point,
            href: xmlResult.Href!
        );
        
        _xmlResultMap[guid] = xmlResult;
        
        var clashObjects = xmlResult.ClashObjects?.Items?
            .Select(ConvertClashObject)
            .ToList() ?? [];
        
        clashObjects.ForEach(result.Objects.Add);
        return result;
    }

    private ClashObject ConvertClashObject(Structure.ClashObject xmlObject)
    {
        int? elementId = null;
        var elementTag = xmlObject.SmartTags?.FirstOrDefault(t => 
            t.Name!.Equals("ElementID", StringComparison.OrdinalIgnoreCase));
        
        if (elementTag != null && int.TryParse(elementTag.Value, out int id))
        {
            elementId = id;
        }

        return new ClashObject(
            layer: xmlObject.Layer ?? "Unknown",
            objectAttribute: ConvertObjectAttribute(xmlObject.ObjectAttribute),
            smartTags:
            [
                ConvertSmartTag(xmlObject.SmartTags?[0]),
                ConvertSmartTag(xmlObject.SmartTags?[1])
            ]
        );
    }

    private ObjectAttribute ConvertObjectAttribute(Structure.ObjectAttribute? xmlObject)
    {
        return new ObjectAttribute(
            name: xmlObject!.Name!,
            value: xmlObject.Value!);
    }
    
    private SmartTag ConvertSmartTag(Structure.SmartTag? xmlObject)
    {
        return new SmartTag(
            name: xmlObject?.Name!,
            value: xmlObject?.Value!);
    }
    
    private int ParseInt(string? value) => 
        int.TryParse(value, out int result) ? result : 0;
        
    private Guid ParseGuid(string? value) => 
        Guid.TryParse(value, out Guid guid) ? guid : Guid.NewGuid();
}