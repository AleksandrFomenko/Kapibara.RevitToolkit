using System.Diagnostics;
using System.IO;
using System.Text;

namespace VentilationInstallations.Report;

public static class WarningsReportService
{
    public static void ShowReportInBackground(IReadOnlyList<string> warnings)
    {
        if (warnings.Count == 0) return;
        
        var snapshot = warnings.ToArray();

        var thread = new Thread(() => WriteAndOpenSafe(snapshot))
        {
            IsBackground = true,
            Name = "Kapibara.WarningsReport"
        };
        thread.Start();
    }

    private static void WriteAndOpenSafe(string[] warnings)
    {
        try
        {
            var fileName = $"Kapibara_VentilationInstallations_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var path = Path.Combine(Path.GetTempPath(), fileName);

            var sb = new StringBuilder();
            sb.AppendLine($"Отчёт от {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Предупреждений: {warnings.Length}");
            sb.AppendLine(new string('-', 60));

            foreach (var warning in warnings)
                sb.AppendLine(warning);

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            Console.WriteLine($"WarningsReportService: {e}");
        }
    }
}