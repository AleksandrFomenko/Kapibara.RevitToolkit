namespace Axes.ViewModels;

public partial class Choice : ObservableObject
{
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private bool _visibleAxes;
    public bool VisibleStartEnd => !VisibleAxes;
    [ObservableProperty] private double _windowHeight;
    [ObservableProperty] private double _windowWidth;

    public static List<Choice?> GetChoices()
    {
        List<Choice?> choices = [
            new Choice()
            {
                DisplayName = "По осям",
                VisibleAxes = true,
                WindowHeight = 500,
                WindowWidth = 300
            }, 
            new Choice()
            {
                DisplayName = "По началу/концу",
                VisibleAxes = false,
                WindowHeight = 400,
                WindowWidth = 400
            }, 
        ];
        return choices;
    }
}

