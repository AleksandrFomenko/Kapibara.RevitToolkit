using AxesLevels.Models;
using ProjectAxes.Abstractions;

namespace ProjectAxes.Factories;

public interface IViewModelFactory
{
    IViewModel Create<TModel>(TModel model)
        where TModel : class, IModel;
}
