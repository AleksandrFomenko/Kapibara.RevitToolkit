using ExporterModels.Dialogs.Instruction.ViewModel;
using Kapibara.Core;

namespace ExporterModels.Dialogs.Instruction.View;

public partial class InstructionView
{
    public InstructionView(InstructionViewModel viewModel, IThemeWatcherService themeWatcherService)
    {
        DataContext = viewModel;
        InstructionViewModel.CloseWindow = Close;
        themeWatcherService.Watch(this);
        InitializeComponent();
    }
}