using System.Windows;
using System.Windows.Threading;
using Kapibara.RevitToolKit.Core.ProgressBar.view;
using Kapibara.RevitToolKit.Core.ProgressBar.viewModel;

namespace Kapibara.RevitToolKit.Core.ProgressBar;

public static class ProgressBar
{
    public static async Task<IProgressHandle> ShowAsync(int max, string title = "Подготовка...")
    {
        var tcs = new TaskCompletionSource<IProgressHandle>();

        var uiThread = new Thread(() =>
        {
            try
            {
                var vm = new ProgressBarViewModel(max) { Message = title };
                var view = new ProgressBarView(vm)
                {
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                var threadDispatcher = Dispatcher.CurrentDispatcher;
                
                view.Loaded += (s, e) =>
                {
                    tcs.SetResult(new ProgressHandle(view, vm, threadDispatcher));
                };
                view.Show();
               

                Dispatcher.Run();
                
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.IsBackground = true;
        uiThread.Start();

        return await tcs.Task;
    }
    
    private class ProgressHandle(Window view, ProgressBarViewModel vm, Dispatcher dispatcher)
        : IProgressHandle
    {
        public void Report((int val, string msg) value)
        {
            dispatcher.BeginInvoke(() =>
            {
                vm.CurrentProgress = value.val;
                vm.Message = value.msg;
            });
        }

        public async Task CloseAsync()
        {
            if (dispatcher != null && !dispatcher.HasShutdownStarted)
            {
                await dispatcher.InvokeAsync(() =>
                {
                    view.Close();
                    dispatcher.InvokeShutdown();
                });
            }
        }

        public void Dispose()
        {
            _ = CloseAsync();
        }
    }
}