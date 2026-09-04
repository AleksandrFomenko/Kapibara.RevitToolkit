using RiserMate.Entities;

namespace RiserMate.Abstractions;

public interface IModelRiserCreator
{
    
    public List<string> GetUserParameters();
    public List<HeatingRiser> GetHeatingRisers(string parameter);
    public List<string> GetTypes3D();
    public List<string> GetMarksHeatDevice();
    public List<string> GetMarksPipe();
    public List<string> GetMarksPipeAccessory();
    public void SelectHeatingRiser(HeatingRiser e, string parameter);
    public void Show3D(HeatingRiser e);
    Task ExecuteAsync(
        List<HeatingRiser> heatingRisers, 
        string parameter, 
        IProgress<(int val, string msg)> progress = null!);
    
    Task MarkActiveViewAsync(string marksHeatDevice, string marksPipe, string markPipeAccessory);
    Task CreateViewsAsync(
        List<HeatingRiser> heatingRisers,
        string parameterName,
        string viewOption,
        bool isMarking,
        string marksHeatDevice,
        string marksPipe,
        string markPipeAccessory,
        IProgress<(int val, string msg)> progress);
    
}