namespace EngineeringSystems.Dialogs.AddAndRenameGroup.EventArgs;

public class GroupNameEventArgs : System.EventArgs
{
    public string Name { get; }
    public GroupNameDialogResult Result { get; set; } = GroupNameDialogResult.Cancelled;
    public GroupNameEventArgs(string name) => Name = name;
}

public enum GroupNameDialogResult
{
    Success,
    NameIsExist,
    TooShort,
    Cancelled
}
