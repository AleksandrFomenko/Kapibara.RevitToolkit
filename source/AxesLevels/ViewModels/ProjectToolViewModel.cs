using AxesLevels.Models;
using ProjectAxes.Common;
using ProjectAxes.ViewModels;

namespace AxesLevels.ViewModels;

public sealed partial class ProjectToolViewModel : ToolViewModel
{
    private readonly IModel _model;

    public ProjectToolViewModel(HeaderInfo header, IModel model)
        : base(header)
    {
        _model = model;
    }
    [RelayCommand]
    private async Task Execute()
    {
        switch (Option?.Type)
        {
            case OptionType.All:
                await _model.DoAllAsync(BeginningIsChecked, EndIsChecked);
                break;
            case OptionType.Selection:
                await _model.DoSelectionAsync(BeginningIsChecked, EndIsChecked);
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}