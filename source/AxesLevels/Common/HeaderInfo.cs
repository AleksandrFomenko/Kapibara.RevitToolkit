namespace ProjectAxes.Common;

public sealed class HeaderInfo
{
    private const string DefaultStart = "Начало";
    private const string DefaultEnd   = "Конец";
    private const string DefaultSelectionsDescription   = "Все на виде или выбранные";
    private const string DefaultHeaderSettingsDescription = "Укажите сторону";

    public string Title { get; }
    public string Selections { get; }
    public string SelectionsDescription { get; }
    public string SettingsDescription { get; }
    public string StartLabel { get; }
    public string EndLabel { get; }

    public HeaderInfo(string title, string selections)
        : this(title, selections, DefaultSelectionsDescription, DefaultStart, DefaultEnd, DefaultHeaderSettingsDescription) { }

    public HeaderInfo(string title, string selections, string selectionsDescription, string startLabel, string endLabel, string settingsDescription)
    {
        Title = title;
        Selections = selections;
        SelectionsDescription = selectionsDescription;
        StartLabel = startLabel;
        EndLabel = endLabel;
        SettingsDescription = settingsDescription;
    }
}