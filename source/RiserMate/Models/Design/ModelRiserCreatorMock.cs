using RiserMate.Abstractions;
using RiserMate.Entities;

namespace RiserMate.Models.Design;

public class ModelRiserCreatorMock : IModelRiserCreator
{
    public List<string> GetUserParameters()
    {
        return ["Параметр 1", "Параметр 2", "Параметр 3"];
    }

    public List<HeatingRiser> GetHeatingRisers(string name)
    {
        var result = new List<HeatingRiser>();

        for (var i = 0; i < 3; i++) result.Add(new HeatingRiser($"Ст {i}"));
        return result;
    }

    public List<string> GetMarksHeatDevice()
    {
        return ["Марка 1", "Марка 2", "Марка 3"];
    }

    public List<string> GetMarksPipe()
    {
        return ["Марка 1", "Марка 2", "Марка 3"];
    }

    public List<string> GetMarksPipeAccessory()
    {
        return ["Марка 1", "Марка 2", "Марка 3"];
    }

    List<string> IModelRiserCreator.GetTypes3D()
    {
        return ["Тип 1", "Тип 2", "Тип 3"];
    }

    public void SelectHeatingRiser(HeatingRiser e, string parameter)
    {
        Console.WriteLine($"Выделить трубу у {e.Name}");
    }

    public void Show3D(HeatingRiser e)
    {
        Console.WriteLine($"Показал {e.Name}");
    }

    public async Task ExecuteAsync(List<HeatingRiser> heatingRisers, string parameter,
        IProgress<(int val, string msg)> progress = null!)
    {
        for (var i = 0; i < heatingRisers.Count; i++)
        {
            progress?.Report((i + 1, $"Обработка стояка {i + 1} из {heatingRisers.Count}"));
            await Task.Delay(3000);
        }
    }

    public Task MarkActiveViewAsync(string marksHeatDevice, string marksPipe, string markPipeAccessory)
    {
        throw new NotImplementedException();
    }

    public Task CreateViewsAsync(List<HeatingRiser> heatingRisers, string parameterName, string viewOption,
        bool isMarking,
        string marksHeatDevice, string marksPipe, string markPipeAccessory, IProgress<(int val, string msg)> progress)
    {
        throw new NotImplementedException();
    }


    public void СreateViews(List<HeatingRiser> heatingRisers, string parameterName, string viewOption, bool isMarking,
        string marksHeatDevice,
        string marksPipe, string markPipeAccessory)
    {
        Console.WriteLine("Создаю виды");
    }
}