using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kapibara.RevitToolKit.Core.ProgressBar.viewModel;

public class ProgressBarViewModel (int maxProgress) : INotifyPropertyChanged
{

    private string _message = "Подготовка...";
    
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }
    
    private int _maxProgress = maxProgress;
    
    public int MaxProgress
    {
        get => _maxProgress;
        set => SetField(ref _maxProgress, value);
    }
    
    private int _currentProgress;

    public int CurrentProgress
    {
        get => _currentProgress;
        set => SetField(ref _currentProgress, value);
    }
    
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}