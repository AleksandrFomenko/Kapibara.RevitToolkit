using System.Windows;
using ExporterModels.Abstractions;

namespace ExporterModels.services;

public sealed class WindowOwnerProvider : IWindowOwnerProvider
{
    private Window? _owner;

    public Window? GetOwner()
    {
        return _owner;
    }

    public void SetOwner(Window? window)
    {
        _owner = window;
    }
}