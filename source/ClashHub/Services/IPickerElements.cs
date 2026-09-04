namespace ClashHub.Services;

public interface IPickerElements
{
    void PickElement(long id);
    void PickElements(IEnumerable<long> ids);
}