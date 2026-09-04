using Kapibara.Core;
using Nice3point.Revit.Toolkit.External;
using RiserMate.Abstractions;
using RiserMate.Core;
using RiserMate.Entities;
using RiserMate.Implementation;


namespace RiserMate.Models.Revit;

public partial class ModelRiserMateCreator(
    IViewCreationService viewCreationService,
    IFilterCreationService filterCreationService) : IModelRiserCreator
{
    private static Document Document => RevitContext.ActiveDocument ?? throw  new NullReferenceException();

    public List<string> GetUserParameters() 
        => Document.GetProjectParameterNames(BuiltInCategory.OST_PipeCurves, ParameterBindingKind.Instance).ToList();

    public List<HeatingRiser> GetHeatingRisers(string parameter)
    {
        var pipes = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_PipeCurves)
            .WhereElementIsNotElementType()
            .ToElements();

        var hashSet = new HashSet<string>();

        foreach (var pipe in pipes)
        {
            var rise = GetParameterValue(pipe, parameter);
            if (rise == string.Empty) continue;
            hashSet.Add(GetParameterValue(pipe, parameter));
        }

        return hashSet.Select(p => new HeatingRiser(p)).ToList();
    }

    public List<string> GetMarksHeatDevice()
    {
        return GetMarks(BuiltInCategory.OST_MechanicalEquipmentTags);
    }

    public List<string> GetMarksPipe()
    {
        return GetMarks(BuiltInCategory.OST_PipeTags);
    }

    public List<string> GetMarksPipeAccessory()
    {
        return GetMarks(BuiltInCategory.OST_PipeAccessoryTags);
    }

    public Task ExecuteAsync(List<HeatingRiser> heatingRisers, string parameter, IProgress<(int val, string msg)> progress = null!)
    {
        throw new NotImplementedException();
    }

    public async Task MarkActiveViewAsync(string marksHeatDevice, string marksPipe, string markPipeAccessory)
    {
            if (Document!.ActiveView is not View3D view)
                return;

            using var t = new Transaction(Document, "RiserMate: Маркировка активного вида");

            try
            {
                t.Start();

                var service = new LabelingService(view);

                if (!string.IsNullOrEmpty(marksHeatDevice))
                    service.MarkHeatDevice(marksHeatDevice);

                if (!string.IsNullOrEmpty(marksPipe))
                    service.MarkPipe(marksPipe);

                if (!string.IsNullOrEmpty(markPipeAccessory))
                    service.MarkPipeAccessory(markPipeAccessory);

                t.Commit();
            }
            catch (Exception ex)
            {
                if (t.GetStatus() == TransactionStatus.Started) t.RollBack();
                Console.WriteLine($"Ошибка маркировки активного вида: {ex.Message}");
            }
    }

    Task IModelRiserCreator.CreateViewsAsync(List<HeatingRiser> heatingRisers, string parameterName, string viewOption, bool isMarking,
        string marksHeatDevice, string marksPipe, string markPipeAccessory, IProgress<(int val, string msg)> progress)
    {
        throw new NotImplementedException();
    }


    private static List<string> GetMarks<T>(T category) where T : Enum
    {
        if (typeof(T) != typeof(BuiltInCategory))
            throw new ArgumentException("Only BuiltInCategory is supported.");

        var builtInCategory = (BuiltInCategory)(object)category;

        var marks = new FilteredElementCollector(Document)
            .OfCategory(builtInCategory)
            .WhereElementIsElementType()
            .ToElements();

        return marks
            .Select(e => e.Name)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }


    public List<string> GetTypes3D()
    {
        var types = new FilteredElementCollector(RevitContext.ActiveDocument)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .Where(v => v.ViewFamily == ViewFamily.ThreeDimensional)
            .Where(v => v.DefaultTemplateId == ElementId.InvalidElementId)
            .Select(v => v.Name)
            .ToList();
        return types.Count == 0 ? ["Тип 3D вида не найден"] : types;
    }

    private static string GetParameterValue(Element elem, string parameterName)
    {
        return elem.LookupParameter(parameterName)?.AsString() ?? string.Empty;
    }

    public void SelectHeatingRiser(HeatingRiser e, string parameter)
    {
        var elements = new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .Where(pipe => pipe.LookupParameter(parameter)?.AsString() == e.Name)
            .ToList();

        var sel = RevitContext.ActiveUiDocument?.Selection;

        sel?.SetElementIds(elements.Select(x => x.Id).ToList());
    }

    public void Show3D(HeatingRiser e)
    {
        Console.WriteLine("Show3D");
    }
    
    
    
    [ExternalEvent]
    private void ExecuteRiserMate(
        List<HeatingRiser> heatingRisers, 
        string parameter,
        IProgress<(int val, string msg)> progress
        )
    {
            using var t = new Transaction(Document, "RiserMate: Обработка стояков");

            try
            {
                t.Start();

                for (var i = 0; i < heatingRisers.Count; i++)
                {
                    var riser = heatingRisers[i];
                    progress?.Report((i + 1, $"Обработка: {riser.Name}"));

                    try
                    {
                        var pip = RiserMateCore.GetBottomPipesByRiser(riser.Name, parameter);
                        if (pip != null) RiserMateCore.Execute(pip, riser.Name, parameter);
                    }
                    catch (Exception localEx)
                    {
                        Console.WriteLine($"Ошибка на стояке {riser.Name}: {localEx.Message}");
                    }
                }

                t.Commit();
            }
            catch (Exception ex)
            {
                if (t.GetStatus() == TransactionStatus.Started) t.RollBack();

                throw new Exception($"Критическая ошибка транзакции: {ex.Message}", ex);
            }
    }
    [ExternalEvent]
    private void CreateViewsAsync(
        List<HeatingRiser> heatingRisers,
        string parameterName,
        string viewOption,
        bool isMarking,
        string marksHeatDevice,
        string marksPipe,
        string markPipeAccessory,
        IProgress<(int val, string msg)> progress)
    {

            using var t = new Transaction(Document, "RiserMateCreateViews");

            try
            {
                t.Start();

                for (var i = 0; i < heatingRisers.Count; i++)
                {
                    var heatingRiser = heatingRisers[i];

                    progress?.Report((i + 1, $"Создание вида: {heatingRiser.Name}"));


                    var view = viewCreationService.CreateView3D(heatingRiser.Name, viewOption);
                    if (view == null) continue;


                    var filter = filterCreationService.CreateFilter(parameterName, heatingRiser.Name);
                    if (filter != null)
                    {
                        view.AddFilter(filter.Id);
                        view.SetFilterVisibility(filter.Id, false);
                    }

                    Document?.Regenerate();
                    
                    
                    if (isMarking)
                    {
                        var service = new LabelingService(view);
                        try
                        {
                            if (!string.IsNullOrEmpty(marksHeatDevice))
                                service.MarkHeatDevice(marksHeatDevice);

                            if (!string.IsNullOrEmpty(marksPipe))
                                service.MarkPipe(marksPipe);
                            
                            if (!string.IsNullOrEmpty(markPipeAccessory))
                                service.MarkPipeAccessory(markPipeAccessory);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Ошибка маркировки {heatingRiser.Name}: {e.Message}");
                        }
                    }
                }

                t.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в CreateViewsAsync {ex.Message}");
                if (t.GetStatus() == TransactionStatus.Started) t.RollBack();
                throw;
            }
    }
}