using Marking.Entities;

namespace Marking.Abstractions;

public interface IMarkingModel
{ 
    event EventHandler<CustomEventArgs> SendName;
    event Action SendMaximize;
    event Action<int>? SendQuantity;
    int PickMark();
    void SetSelectedChoice(Choice? choice);
    Task ExecuteMarkingAsync(int elementId);
}

public class CustomEventArgs(string name) : EventArgs 
{ 
    public string Name {get; set;} = name;
}