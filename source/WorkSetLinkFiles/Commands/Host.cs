using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using WorkSetLinkFiles.Models;
using WorkSetLinkFiles.ViewModels;
using WorkSetLinkFiles.Views;

namespace WorkSetLinkFiles.Commands;

public static class Host
{
   public static void Start()
   {
      var services = new ServiceCollection();
      var doc = RevitContext.ActiveDocument;
      if (doc == null)  return;

      services.AddSingleton(doc); 
      services.AddSingleton<WorkSetLinkFilesViewModel>();
      services.AddSingleton<WorkSetLinkFilesModel>();
      services.AddSingleton<WorkSetLinkFilesView>();
      services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
      var serviceProvider = services.BuildServiceProvider();
      var view = serviceProvider.GetRequiredService<WorkSetLinkFilesView>();
      var tws = serviceProvider.GetRequiredService<IThemeWatcherService>();
      view.Loaded += async (sender, args) => tws.ApplyTheme();
      view.SourceInitialized += (sender, args) => tws.ApplyTheme();
      view.ShowDialog();
   }
}