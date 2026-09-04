using System.Collections.ObjectModel;
using Axes.Models;

namespace Axes.ViewModels;

public sealed partial class AxesViewModel : ObservableObject
{
    private readonly AxesModel _model;
    
    [ObservableProperty] private List<Choice?> _choices = null!;
    [ObservableProperty] private Choice? _selectedChoice;
    
    [ObservableProperty] private ObservableCollection<Option> _options = null!;
    [ObservableProperty] private Option? _selectedOption;

    [ObservableProperty] private bool _top;
    [ObservableProperty] private bool _left;
    [ObservableProperty] private bool _right;
    [ObservableProperty] private bool _bottom;

    private bool _isAll;
    
    public AxesViewModel(AxesModel model)
    {
        _model         = model;
        Choices        = Choice.GetChoices();
        SelectedChoice = Choices.FirstOrDefault();
        
        Options        = new ObservableCollection<Option>(
            Enum.GetValues(typeof(Option)).Cast<Option>());
        SelectedOption = Options[0];
    }

    partial void OnSelectedOptionChanged(Option? value) => _isAll = value == Option.All;
    
    partial void OnTopChanged(bool value) => 
        _= _model.HideTopOrBottomAsyncEvent.RaiseAsync(_isAll, true, value);
    partial void OnBottomChanged(bool value) => 
        _= _model.HideTopOrBottomAsyncEvent.RaiseAsync(_isAll, false, value);
    partial void OnLeftChanged(bool value) => 
        _= _model.HideRightOrLeftAsyncEvent.RaiseAsync(_isAll, true, value);
    partial void OnRightChanged(bool value) => 
        _ = _model.HideRightOrLeftAsyncEvent.RaiseAsync(_isAll, false, value);

    [RelayCommand]
    private void To2D() => 
        _ = _model.ChangeDatumExtentAsyncEvent.RaiseAsync(_isAll, false);
    
    [RelayCommand]
    private void To3D() => 
        _ = _model.ChangeDatumExtentAsyncEvent.RaiseAsync(_isAll, true);
}