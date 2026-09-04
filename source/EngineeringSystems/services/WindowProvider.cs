using System.Windows;

namespace EngineeringSystems.services;

public class WindowProvider
{
    private Window? _windowOwner;
    public Window WindowOwner => 
        _windowOwner ?? throw new InvalidOperationException("Window owner is not set");
    
    public void SetWindowOwner(Window window)
    {
        _windowOwner = window;
    }
}