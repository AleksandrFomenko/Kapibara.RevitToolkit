namespace ClashHub.Models.Parsers;

public interface IChangeTracker
{
    event Action<Guid, string> StatusChanged;
    void NotifyStatusChanged(Guid id, string newStatus);
}