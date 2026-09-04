namespace LevelByFloor.Models;

public partial class Options : ObservableObject
{
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private FilteredElementCollector _fec;

    internal Options(string name, FilteredElementCollector fec)
    {
        _name = name;
        _fec = fec;
    }
}