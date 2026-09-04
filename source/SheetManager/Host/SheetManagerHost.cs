using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using SheetManager.ViewModels;
using SheetManager.Views;
using Wpf.Ui;
using Wpf.Ui.Abstractions;


namespace SheetManager.Host;

public static  class SheetManagerHost
{
     private static IServiceProvider? _serviceProvider;
     public static IServiceProvider ServiceProvider => _serviceProvider ?? 
                                                       throw new InvalidOperationException("Host not started");
     
     private static IServiceScope? _scope;
     public static IServiceScope Scope => _scope ?? 
                                          throw new InvalidOperationException("Host not started");
     
     public static void Build()
     {
          var services = new ServiceCollection();
          
          // Темы
          services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
          services.AddScoped<INavigationViewPageProvider, PageService>();
          
          // Views
          services.AddScoped<SheetManagerView>();
          services.AddScoped<CreateSheetsView>();
          services.AddScoped<PrintSheetsView>();
          
          // ViewModels
          services.AddScoped<SheetManagerViewModel>();
          services.AddScoped<CreateSheetsViewModel>();
          services.AddScoped<PrintSheetsViewModel>();
          
          // Data
          //todo
          //
          // Models
          //todo
          //
          _serviceProvider = services.BuildServiceProvider();
     }

     public static void Run()
     {
         if (_serviceProvider is null)
             throw new InvalidOperationException("Host not started");
         
         _scope = _serviceProvider.CreateScope();
         
         var sp = _scope.ServiceProvider;
         
         var tws = sp.GetService<IThemeWatcherService>();
         var view = sp.GetService<SheetManagerView>();
         
         tws?.Initialize();
         view?.ShowDialog();
     }

     public static T GetService<T>() where T : class =>  _serviceProvider!.GetRequiredService<T>();

}