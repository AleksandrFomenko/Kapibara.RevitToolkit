namespace ExporterModels.Progress;

public class ProgressInfo
{
    public ProgressInfo(int current, int total, string fileName)
    {
        Current = current;
        Total = total;
        FileName = fileName;
    }

    public int Current { get; }
    public int Total { get; }
    public string FileName { get; }

    public override string ToString()
    {
        return $"{FileName} ({Current}/{Total})";
    }
}