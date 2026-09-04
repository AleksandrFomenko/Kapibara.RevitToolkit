using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClashHub.Domain.Entities;

public partial class ClashTest : ObservableObject
{
    public string Name { get; }
    public string TestType { get; }
    
    [ObservableProperty] private ClashSummary _summary;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
     private List<ClashResult> _results = [];

     
    partial void OnResultsChanged(List<ClashResult> value)
    {
        RecalculateSummary();
    }

    public ClashTest(string name, string testType, ClashSummary summary)
    {
        Name = name;
        TestType = testType;
        Summary = summary;
    }
    public void RecalculateSummary()
    {
        Summary.Update(
            total: Results.Count,
            @new: Results.Count(r => r.Status == "New"),
            active: Results.Count(r => r.Status == "Active"),
            reviewed: Results.Count(r => r.Status == "Reviewed"),
            approved: Results.Count(r => r.Status == "Approved"),
            resolved: Results.Count(r => r.Status == "Resolved")
        );
    }
}

public partial class ClashSummary : ObservableObject
{
    [ObservableProperty] private int _total;
    [ObservableProperty] private int _new;
    [ObservableProperty] private int _active;
    [ObservableProperty] private int _reviewed;
    [ObservableProperty] private int _approved;
    [ObservableProperty] private int _resolved;
    public string TestType { get; set; }
        
    public ClashSummary(int total, int @new, int active, int reviewed, int approved, int resolved, string testType)
    {
        Total = total;
        New = @new;
        Active = active;
        Reviewed = reviewed;
        Approved = approved;
        Resolved = resolved;
        TestType = testType;
    }
    
    public void Update(
        int total, int @new, int active, int reviewed, int approved, int resolved)
    {
        Total = total;
        New = @new;
        Active = active;
        Reviewed = reviewed;
        Approved = approved;
        Resolved = resolved;
    }
} 

public sealed partial class ClashResult : ObservableObject
{
    public Guid Id { get; }
    public string Name { get; }
    
    [ObservableProperty] 
    private string _status;

    [ObservableProperty] private string _statusFromList = null!;
    public double Distance { get; }
    public DateTime Created { get; }
    public Point3D ClashPoint { get; }
    public List<ClashObject> Objects { get; } = [];
    public string Href { get; set; }

    [ObservableProperty]
    private ObservableCollection<string> _statusList =
    [
        "New",
        "Active",
        "Reviewed",
        "Approved",
        "Resolved"
    ];

    partial void OnStatusFromListChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Status = value;
        }
    }
    public ClashResult(
        Guid id, 
        string name,
        string status, 
        double distance, 
        DateTime created, 
        Point3D clashPoint, 
        string href)
    {
        Id = id; 
        Name = name;
        Status = status;
        Distance = distance;
        Created = created;
        ClashPoint = clashPoint;
        Href = href;

        var lowerStatus = Status?.ToLowerInvariant();
        var lowerStatusList = StatusList.Select(s => s?.ToLowerInvariant()).ToList();

        if (!string.IsNullOrEmpty(lowerStatus) && lowerStatusList.Contains(lowerStatus))
        {
            var originalStatus = StatusList.FirstOrDefault(s => 
                s.Equals(Status, StringComparison.OrdinalIgnoreCase));
            if (originalStatus != null) StatusFromList = originalStatus;
        }
        else
        {
            StatusFromList = null!;
        }
    }

    public event Action ShowEvent = null!;
    public event Action ShowIsolatedEvent = null!;
    
    [RelayCommand]
    private void ClickShow() => ShowEvent?.Invoke();

    [RelayCommand]
    private void ClickShowIsolated() => ShowIsolatedEvent?.Invoke();
}

public class ClashObject
{
    public string Layer { get; }
    public ObjectAttribute ObjectAttribute { get; set; }
    public List<SmartTag> SmartTags { get; set; }
    public ClashObject(string layer, ObjectAttribute objectAttribute, List<SmartTag> smartTags)
    {
        Layer = layer;
        ObjectAttribute = objectAttribute;
        SmartTags = smartTags;
    }
}

public class ObjectAttribute
{
    public string Name { get; set; }
    public string Value { get; set; }
    
    public ObjectAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class SmartTag
{
    public string Name { get; set; }
    
    public string Value { get; set; }
    
    public SmartTag(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class Point3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public Point3D(double x, double y, double z)
    { 
        X = x; 
        Y = y;
        Z = z;
    }
}
