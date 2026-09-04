namespace ClashHub.Services;

public interface IClashViewCreator
{
    void CreateViewAsync(long firstElementId, long secondElementId);
}