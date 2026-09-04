namespace RiserMate.Abstractions;

public interface IConfigRiserMate
{
    public string GetSelectedUserParameter();
    public void SetSelectedUserParameter(string value);
}