namespace ClashHub.Models.Parsers;

public class ChangeTracker : IChangeTracker
{
    public event Action<Guid, string>? StatusChanged;
    
    public void NotifyStatusChanged(Guid id, string newStatus)
    {
        StatusChanged?.Invoke(id, newStatus);
    }
}