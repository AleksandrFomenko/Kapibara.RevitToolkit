using ExporterModels.ViewModels;

namespace ExporterModels.Progress;

public class UiProgress<T> : IProgress<T>
{
    private readonly int _total;
    private readonly ExporterModelsViewModel _vm;

    public UiProgress(ExporterModelsViewModel vm, int total)
    {
        _vm = vm;
        _total = total;
        _vm.ProgressMaximum = total;
        _vm.ProgressValue = 0;
        _vm.ProgressIsIndeterminate = false;
    }

    public void Report(T value)
    {
        _vm._ui.Post(_ =>
        {
            _vm.ProgressValue++;
            _vm.ProgressMessage = value.ToString();
        }, null);
    }
}