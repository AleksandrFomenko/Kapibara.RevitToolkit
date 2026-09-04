namespace Kapibara.RevitToolKit.Core.ProgressBar;

public interface IProgressHandle : IProgress<(int val, string msg)>, IDisposable
{
    Task CloseAsync();
}