using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PerformanceCenter;

public partial class MainWindow
{
    private async void OnDashboardRefreshClick(object? sender, RoutedEventArgs e)
    {
        await AtualizarDashboardRapidoAsync();
    }

    private async Task AtualizarDashboardRapidoAsync()
    {
        try
        {
            SetDashboardInfo(DashboardRedeValorText, DashboardRedeStatusText, "...", "medindo");
            SetDashboardInfo(DashboardGpuValorText, DashboardGpuStatusText, "...", "lendo");
            SetDashboardInfo(DashboardCpuValorText, DashboardCpuStatusText, "...", "lendo");
            SetDashboardInfo(DashboardRamValorText, DashboardRamStatusText, "...", "lendo");
            SetDashboardInfo(DashboardWindowsValorText, DashboardWindowsStatusText, "OK?", "clique para verificar");

            long? pingMs = await MedirPingDashboardAsync();

            if (pingMs is null)
            {
                SetDashboardAlert(DashboardRedeValorText, DashboardRedeStatusText, "-- ms", "sem resposta");
            }
            else
            {
                string statusPing = ClassificarPingDashboard(pingMs.Value);
                AplicarStatusDashboard(DashboardRedeValorText, DashboardRedeStatusText, $"{pingMs.Value} ms", statusPing);
            }

            var leituras = ColetarSensoresHardware();

            var temperaturas = leituras
                .Where(x => x.TipoSensor == "Temperature")
                .Where(x => x.Valor > 0)
                .Where(x => !EhLimiteTemperaturaDashboard(x.Sensor))
                .ToList();

            var cargas = leituras
                .Where(x => x.TipoSensor == "Load")
                .ToList();

            var gpuTemp = temperaturas
                .Where(x => x.TipoHardware.Contains("Gpu", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            var cpuLoad = cargas
                .Where(x => x.TipoHardware.Contains("Cpu", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            var ramLoad = cargas
                .Where(x => x.TipoHardware.Contains("Memory", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            if (gpuTemp is not null)
                AplicarStatusDashboard(DashboardGpuValorText, DashboardGpuStatusText, $"{gpuTemp.Valor:0} °C", ClassificarTemperaturaDashboard(gpuTemp.Valor));
            else
                SetDashboardInfo(DashboardGpuValorText, DashboardGpuStatusText, "-- °C", "não exposto");

            if (cpuLoad is not null)
                AplicarStatusDashboard(DashboardCpuValorText, DashboardCpuStatusText, $"{cpuLoad.Valor:0}%", ClassificarUsoDashboard(cpuLoad.Valor));
            else
                SetDashboardInfo(DashboardCpuValorText, DashboardCpuStatusText, "-- %", "não exposto");

            if (ramLoad is not null)
                AplicarStatusDashboard(DashboardRamValorText, DashboardRamStatusText, $"{ramLoad.Valor:0}%", ClassificarRamDashboard(ramLoad.Valor));
            else
                SetDashboardInfo(DashboardRamValorText, DashboardRamStatusText, "-- %", "não exposto");

            SetDashboardInfo(DashboardWindowsValorText, DashboardWindowsStatusText, "OK?", "verificar manualmente");
        }
        catch
        {
            SetDashboardAlert(DashboardRedeValorText, DashboardRedeStatusText, "--", "erro");
            SetDashboardAlert(DashboardGpuValorText, DashboardGpuStatusText, "--", "erro");
            SetDashboardAlert(DashboardCpuValorText, DashboardCpuStatusText, "--", "erro");
            SetDashboardAlert(DashboardRamValorText, DashboardRamStatusText, "--", "erro");
        }
    }

    private static void AplicarStatusDashboard(Avalonia.Controls.TextBlock valor, Avalonia.Controls.TextBlock status, string valorTexto, string statusTexto)
    {
        string s = statusTexto.ToLowerInvariant();

        if (s.Contains("excelente") || s == "bom" || s == "ok")
            SetDashboardOk(valor, status, valorTexto, statusTexto);
        else if (s.Contains("atenção") || s.Contains("alta"))
            SetDashboardWarning(valor, status, valorTexto, statusTexto);
        else if (s.Contains("alto") || s.Contains("crítica") || s.Contains("erro") || s.Contains("sem resposta"))
            SetDashboardAlert(valor, status, valorTexto, statusTexto);
        else
            SetDashboardInfo(valor, status, valorTexto, statusTexto);
    }

    private static void SetDashboardOk(Avalonia.Controls.TextBlock valor, Avalonia.Controls.TextBlock status, string valorTexto, string statusTexto)
    {
        valor.Text = valorTexto;
        status.Text = statusTexto;
        valor.Foreground = Brush.Parse("#86EFAC");
        status.Foreground = Brush.Parse("#86EFAC");
    }

    private static void SetDashboardWarning(Avalonia.Controls.TextBlock valor, Avalonia.Controls.TextBlock status, string valorTexto, string statusTexto)
    {
        valor.Text = valorTexto;
        status.Text = statusTexto;
        valor.Foreground = Brush.Parse("#FDE68A");
        status.Foreground = Brush.Parse("#FDE68A");
    }

    private static void SetDashboardAlert(Avalonia.Controls.TextBlock valor, Avalonia.Controls.TextBlock status, string valorTexto, string statusTexto)
    {
        valor.Text = valorTexto;
        status.Text = statusTexto;
        valor.Foreground = Brush.Parse("#FCA5A5");
        status.Foreground = Brush.Parse("#FCA5A5");
    }

    private static void SetDashboardInfo(Avalonia.Controls.TextBlock valor, Avalonia.Controls.TextBlock status, string valorTexto, string statusTexto)
    {
        valor.Text = valorTexto;
        status.Text = statusTexto;
        valor.Foreground = Brush.Parse("#F8FAFC");
        status.Foreground = Brush.Parse("#94A3B8");
    }

    private static async Task<long?> MedirPingDashboardAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("1.1.1.1", 1500);

            if (reply.Status == IPStatus.Success)
                return reply.RoundtripTime;

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool EhLimiteTemperaturaDashboard(string sensor)
    {
        return sensor.Contains("Critical", StringComparison.OrdinalIgnoreCase) ||
               sensor.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
               sensor.Contains("Limit", StringComparison.OrdinalIgnoreCase) ||
               sensor.Contains("TjMax", StringComparison.OrdinalIgnoreCase);
    }

    private static string ClassificarPingDashboard(long ms)
    {
        if (ms <= 30) return "excelente";
        if (ms <= 70) return "bom";
        if (ms <= 120) return "atenção";
        return "alto";
    }

    private static string ClassificarTemperaturaDashboard(float valor)
    {
        if (valor < 75) return "ok";
        if (valor < 85) return "atenção";
        return "alto";
    }

    private static string ClassificarUsoDashboard(float valor)
    {
        if (valor < 70) return "ok";
        if (valor < 90) return "atenção";
        return "alto";
    }

    private static string ClassificarRamDashboard(float valor)
    {
        if (valor < 75) return "ok";
        if (valor < 90) return "alta";
        return "crítica";
    }
}

