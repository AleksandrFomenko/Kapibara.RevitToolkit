using SheetManager.Host;


namespace TestUI;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        SheetManagerHost.Build();
        SheetManagerHost.Run();
    }
}