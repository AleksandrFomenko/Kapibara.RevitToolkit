using System.Collections.ObjectModel;
using Marking.Abstractions;
using Marking.Entities;
using Marking.Models;

namespace Marking.ViewModels;

public sealed partial class MarkingViewModel : ObservableObject
{
    public IMarkingModel Model;

    [ObservableProperty] private ObservableCollection<Choice> _choices;
    [ObservableProperty] private Choice? _selectedChoice;
    [ObservableProperty] private string _title = "Выберите марку";
    
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    [ObservableProperty] private string? _selectedMark;
    
    public event Action? MinimizeWindow;

   
    private int _idMark = 0;

    public MarkingViewModel(MarkingModel model)
    {
        Model = model;
        _choices = new ObservableCollection<Choice>(
            Enum.GetValues<Choice>().Cast<Choice>());
        SelectedChoice = _choices[0];
        Model.SendName += OnSendName;
        Model.SendQuantity += (x) => Title = $"Маркируется {x} элементов";
    }

    partial void OnSelectedChoiceChanged(Choice? value) => Model.SetSelectedChoice(value);
    private void OnSendName(object? sender, CustomEventArgs e) =>  SelectedMark = e.Name;
    private bool CanExecute () => SelectedMark is not null;


    [RelayCommand]
    private void SelectMarkInstance()
    {
        MinimizeWindow?.Invoke();
        _idMark = Model.PickMark();
    } 

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute() => Model.ExecuteMarkingAsync(_idMark);

}