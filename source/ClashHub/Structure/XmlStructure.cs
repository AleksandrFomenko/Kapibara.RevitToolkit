using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClashHub.Structure;

[XmlRoot("exchange")]
public class XmlStructure : ObservableObject
{
    [XmlAttribute("units")]
    public string? Units { get; set; }

    [XmlElement("batchtest")]
    public Batchtest? Batchtest { get; set; }
}

public class Batchtest : ObservableObject
{
    [XmlElement("clashtests")]
    public Clashtests? Clashtests { get; set; }
}

public class Clashtests
{
    [XmlElement("clashtest")]
    public List<Clashtest>? ClashTestList { get; set; }
}

public class Clashtest : ObservableObject
{
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("test_type")]
    public string? TestType { get; set; }
    
    [XmlElement("summary")]
    public Summary? Summary { get; set; }
    
    [XmlElement("clashresults")]
    public ClashResults? ClashResults { get; set; }
}

public class Summary : ObservableObject
{
    [XmlAttribute("total")]
    public string? Total { get; set; }
    
    [XmlAttribute("new")]
    public string? New { get; set; }
    
    [XmlAttribute("active")]
    public string? Active { get; set; }
    
    [XmlAttribute("reviewed")]
    public string? Reviewed { get; set; }
    
    [XmlAttribute("approved")]
    public string? Approved { get; set; }
    
    [XmlAttribute("resolved")]
    public string? Resolved { get; set; }
    
    [XmlElement("testtype")]
    public string? TestType { get; set; }
}

public class ClashResults : ObservableObject
{
    [XmlElement("clashresult")]
    public List<ClashResult>? Results { get; set; }
}

public class ClashResult : ObservableObject 
{
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("guid")]
    public string? Guid { get; set; }

    [XmlAttribute("href")]
    public string? Href { get; set; }

    [XmlAttribute("status")]
    public string Status { get; set; } = null!;

    [XmlAttribute("distance")]
    public double Distance { get; set; }

    [XmlElement("description")]
    public string? Description { get; set; }

    [XmlElement("resultstatus")]
    public string? ResultStatus { get; set; }
    
    [XmlElement("clashpoint")]
    public ClashPoint? ClashPoint { get; set; }

    [XmlElement("gridlocation")]
    public string? GridLocation { get; set; }

    [XmlElement("createddate")]
    public CreatedDate? CreatedDate { get; set; }

    [XmlElement("clashobjects")]
    public ClashObjects? ClashObjects { get; set; }
        
    [XmlIgnore]
    public DateTime Created => new DateTime(
        CreatedDate!.Date!.Year,
        CreatedDate.Date.Month,
        CreatedDate.Date.Day,
        CreatedDate.Date.Hour,
        CreatedDate.Date.Minute,
        CreatedDate.Date.Second);
    }

    public class ClashPoint
    {
        [XmlElement("pos3f")]
        public Pos3F? Pos { get; set; }
    }

    public class Pos3F
    {
        [XmlAttribute("x")]
        public double X { get; set; }

        [XmlAttribute("y")]
        public double Y { get; set; }

        [XmlAttribute("z")]
        public double Z { get; set; }
    }

    public class CreatedDate
    {
        [XmlElement("date")]
        public DateElement? Date { get; set; }
    }

    public class DateElement
    {
        [XmlAttribute("year")]
        public int Year { get; set; }

        [XmlAttribute("month")]
        public int Month { get; set; }

        [XmlAttribute("day")]
        public int Day { get; set; }

        [XmlAttribute("hour")]
        public int Hour { get; set; }

        [XmlAttribute("minute")]
        public int Minute { get; set; }

        [XmlAttribute("second")]
        public int Second { get; set; }
    }

    public class ClashObjects
    {
        [XmlElement("clashobject")]
        public List<ClashObject>? Items { get; set; }
    }

    public class ClashObject
    {
        [XmlElement("objectattribute")]
        public ObjectAttribute? ObjectAttribute { get; set; }

        [XmlElement("layer")]
        public string? Layer { get; set; }

        [XmlArray("smarttags"), XmlArrayItem("smarttag")]
        public List<SmartTag>? SmartTags { get; set; }
    }

    public class ObjectAttribute
    {
        [XmlElement("name")]
        public string? Name { get; set; }

        [XmlElement("value")]
        public string? Value { get; set; }
    }

    public class SmartTag
    {
        [XmlElement("name")]
        public string? Name { get; set; }

        [XmlElement("value")]
        public string? Value { get; set; }
        
    }
