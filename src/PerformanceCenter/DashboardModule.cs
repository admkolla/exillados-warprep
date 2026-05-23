using Avalonia.Interactivity;
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
            DashboardRedeValorText.Text = "...";
            DashboardRedeStatusText.Text = "medindo";

            DashboardGpuValorText.Text = "...";
            DashboardGpuStatusText.Text = "lendo";

            DashboardCpuValorText.Text = "...";
            DashboardCpuStatusText.Text = "lendo";

            DashboardRamValorText.Text = "...";
            DashboardRamStatusText.Text = "lendo";

            DashboardWindowsValorText.Text = "OK?";
            DashboardWindowsStatusText.Text = "clique para verificar";

            long? pingMs = await MedirPingDashboardAsync();

            if (pingMs is null)
            {
                DashboardRedeValorText.Text = "-- ms";
                DashboardRedeStatusText.Text = "sem resposta";
            }
            else
            {
                DashboardRedeValorText.Text = $"{pingMs.Value} ms";
                DashboardRedeStatusText.Text = ClassificarPingDashboard(pingMs.Value);
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
            {
                DashboardGpuValorText.Text = $"{gpuTemp.Valor:0} °C";
                DashboardGpuStatusText.Text = ClassificarTemperaturaDashboard(gpuTemp.Valor);
            }
            else
            {
                DashboardGpuValorText.Text = "-- °C";
                DashboardGpuStatusText.Text = "não exposto";
            }

            if (cpuLoad is not null)
            {
                DashboardCpuValorText.Text = $"{cpuLoad.Valor:0}%";
                DashboardCpuStatusText.Text = ClassificarUsoDashboard(cpuLoad.Valor);
            }
            else
            {
                DashboardCpuValorText.Text = "-- %";
                DashboardCpuStatusText.Text = "não exposto";
            }

            if (ramLoad is not null)
            {
                DashboardRamValorText.Text = $"{ramLoad.Valor:0}%";
                DashboardRamStatusText.Text = ClassificarRamDashboard(ramLoad.Valor);
            }
            else
            {
                DashboardRamValorText.Text = "-- %";
                DashboardRamStatusText.Text = "não exposto";
            }
        }
        catch
        {
            DashboardRedeStatusText.Text = "erro";
            DashboardGpuStatusText.Text = "erro";
            DashboardCpuStatusText.Text = "erro";
            DashboardRamStatusText.Text = "erro";
        }
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
