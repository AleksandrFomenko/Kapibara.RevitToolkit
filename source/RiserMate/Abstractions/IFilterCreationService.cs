namespace RiserMate.Abstractions;

public interface IFilterCreationService
{
    public ParameterFilterElement CreateFilter(string nameParameter, string value);
}