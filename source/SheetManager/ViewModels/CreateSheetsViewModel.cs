using System.Collections.ObjectModel;

namespace SheetManager.ViewModels;

public partial class CreateSheetsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _framesProject;
    [ObservableProperty] private string _selectedFrame;
    [ObservableProperty] private int _numberFrame;
}