using Kapibara.Core;
using Kapibara.RevitToolKit.Core.ProgressBar;
using RiserMate.Abstractions;
using RiserMate.Entities;
using RiserMate.Lookups;

namespace RiserMate.ViewModels;

public partial class RizerCreatorViewModel : ObservableObject
{
    private readonly IModelRiserCreator _model;
    private readonly PluginConfig<RiserMateConfig> _config;

    private List<HeatingRiser> _allHeatingRisers = [];

    [ObservableProperty] private List<Choice> _choices;
    [ObservableProperty] private Choice _selectedChoice;

    [ObservableProperty] private List<string> _userParameters;
    [ObservableProperty] private string _selectedUserParameter;

    [ObservableProperty] private List<HeatingRiser> _heatingRisers = [];

    [ObservableProperty] private bool _checkAllRisers;
    [ObservableProperty] private string? _filterByName;

    [ObservableProperty] private bool _createView;
    [ObservableProperty] private List<string> _viewTypes3D;
    [ObservableProperty] private string _selectedViewType3D = null!;

    [ObservableProperty] private bool _isMarking;
    [ObservableProperty] private bool _visibleMark;

    [ObservableProperty] private List<string> _marksHeatDevice;
    [ObservableProperty] private string _selectedMarkHeatDevice = null!;
    [ObservableProperty] private List<string> _marksPipe;
    [ObservableProperty] private string _selectedMarkPipe = null!;
    [ObservableProperty] private List<string> _marksPipeAccessory;
    [ObservableProperty] private string _selectedMarkPipeAccessory = null!;

    public RizerCreatorViewModel(IModelRiserCreator model, PluginConfig<RiserMateConfig> config)
    {
        _model = model;
        _config = config;

        Choices = Enum.GetValues<Choice>().ToList();
        ViewTypes3D = _model.GetTypes3D();
        MarksHeatDevice = _model.GetMarksHeatDevice();
        MarksPipe = _model.GetMarksPipe();
        MarksPipeAccessory = _model.GetMarksPipeAccessory();

        UserParameters = _model.GetUserParameters();

        var savedParam = _config.Data.SelectedUserParameter;
        SelectedUserParameter = UserParameters.Contains(savedParam) ? savedParam : string.Empty;
        
    }

    private void LoadData()
    {
        if (string.IsNullOrEmpty(SelectedUserParameter)) return;

        _allHeatingRisers = _model.GetHeatingRisers(SelectedUserParameter)
            .OrderBy(x => x.Name)
            .ToList();

        foreach (var riser in _allHeatingRisers)
        {
            riser.ClickSelect += SelectHeatingRiser!;
            riser.ClickShow3D += ShowHeatingRiser!;
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrEmpty(FilterByName))
        {
            HeatingRisers = _allHeatingRisers;
        }
        else
        {
            HeatingRisers = _allHeatingRisers
                .Where(h => h.Name != null && h.Name.IndexOf(FilterByName ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }
    }

    partial void OnSelectedUserParameterChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _config.Data.SelectedUserParameter = value;
        _config.Save();
        LoadData();
    }

    partial void OnFilterByNameChanged(string? value) => ApplyFilter();

    [RelayCommand]
    private void Update() => LoadData();

    private void SelectHeatingRiser(object sender, HeatingRiser e) => _model.SelectHeatingRiser(e, SelectedUserParameter);
    private void ShowHeatingRiser(object sender, HeatingRiser e) => _model.Show3D(e);

    partial void OnCheckAllRisersChanged(bool value) => HeatingRisers.ForEach(r => r.IsChecked = value);
    partial void OnCreateViewChanged(bool value) => SetVisibleMark();
    partial void OnIsMarkingChanged(bool value) => SetVisibleMark();
    private void SetVisibleMark() => VisibleMark = CreateView && IsMarking;

    [RelayCommand]
    private async Task Execute()
    {
        var risersToProcess = SelectedChoice == Choice.All
            ? HeatingRisers.ToList()
            : HeatingRisers.Where(p => p.IsChecked).ToList();

        if (risersToProcess.Count == 0)
        {
            return;
        }

        var progressHandle = await ProgressBar.ShowAsync(risersToProcess.Count, "Обработка стояков...");

        try
        {
            await _model.ExecuteAsync(risersToProcess, SelectedUserParameter, progressHandle);
        }
        catch (Exception)
        {
            await progressHandle.CloseAsync();
            return;
        }
        finally
        {
            await progressHandle.CloseAsync();
        }

        if (CreateView)
        {
            var progressViews = await ProgressBar.ShowAsync(risersToProcess.Count, "Создание 3D видов...");

            try
            {
                await _model.CreateViewsAsync(
                    risersToProcess,
                    SelectedUserParameter,
                    SelectedViewType3D,
                    IsMarking,
                    SelectedMarkHeatDevice,
                    SelectedMarkPipe,
                    SelectedMarkPipeAccessory,
                    progressViews);
            }
            catch (Exception)
            {
                //ignored
            }
            finally
            {
                await progressViews.CloseAsync();
            }
        }
    }

    [RelayCommand]
    private async Task MarkingActiveView()
    {
        await _model.MarkActiveViewAsync(
            SelectedMarkHeatDevice,
            SelectedMarkPipe,
            SelectedMarkPipeAccessory);
    }
}