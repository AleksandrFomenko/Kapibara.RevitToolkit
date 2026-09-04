using Kapibara.RevitToolKit.Core.ProgressBar.viewModel;

namespace Kapibara.RevitToolKit.Core.ProgressBar.view;

public partial class ProgressBarView
{
    public ProgressBarView(ProgressBarViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}