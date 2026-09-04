using System.Windows;

namespace ExporterModels.Abstractions;

public interface IWindowOwnerProvider
{
    Window? GetOwner();
    void SetOwner(Window? window);
}