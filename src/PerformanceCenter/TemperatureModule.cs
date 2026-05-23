using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceCenter;

public partial class MainWindow
{
    private static List<SensorLeitura> ColetarSensoresHardware()
    {
        var leituras = new List<SensorLeitura>();

        try
        {
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true
            };

            computer.Open();

            foreach (var hardware in computer.Hardware)
                AdicionarLeiturasHardware(hardware, leituras);

            computer.Close();
        }
        catch
        {
        }

        return leituras
            .Where(x => !float.IsNaN(x.Valor))
            .OrderBy(x => x.TipoHardware)
            .ThenBy(x => x.Hardware)
            .ThenBy(x => x.TipoSensor)
            .ThenBy(x => x.Sensor)
            .ToList();
    }

    private static void AdicionarLeiturasHardware(IHardware hardware, List<SensorLeitura> leituras)
    {
        try
        {
            hardware.Update();

            foreach (var sub in hardware.SubHardware)
                AdicionarLeiturasHardware(sub, leituras);

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.Value is null)
                    continue;

                string unidade = sensor.SensorType switch
                {
                    SensorType.Temperature => "°C",
                    SensorType.Load => "%",
                    SensorType.Clock => "MHz",
                    SensorType.Fan => "RPM",
                    SensorType.Power => "W",
                    SensorType.Voltage => "V",
                    _ => ""
                };

                if (string.IsNullOrWhiteSpace(unidade))
                    continue;

                leituras.Add(new SensorLeitura(
                    hardware.Name,
                    hardware.HardwareType.ToString(),
                    sensor.Name,
                    sensor.SensorType.ToString(),
                    sensor.Value.Value,
                    unidade
                ));
            }
        }
        catch
        {
        }
    }

    private static string GerarRelatorioTemperaturas(List<SensorLeitura> leituras)
    {
        var sb = new StringBuilder();

        sb.AppendLine("===== Performance Center v3.4.4 =====");
        sb.AppendLine();
        sb.AppendLine("🌡️ TEMPERATURAS E SENSORES GAMER");
        sb.AppendLine();
        sb.AppendLine("IMPORTANTE:");
        sb.AppendLine("- Nem todo PC mostra todos os sensores corretamente.");
        sb.AppendLine("- Temperatura 0°C foi ignorada como leitura inválida.");
        sb.AppendLine("- Warning Temperature e Critical Temperature do NVMe são limites do hardware, não temperatura atual.");
        sb.AppendLine("- O Performance Center apenas lê sensores; não altera fan, clock, voltagem ou driver.");
        sb.AppendLine();

        if (leituras.Count == 0)
        {
            sb.AppendLine("⚠️ Nenhum sensor foi lido.");
            sb.AppendLine("Tente abrir o Performance Center como Administrador e testar novamente.");
            return sb.ToString();
        }

        sb.AppendLine(GerarResumoTemperaturas(leituras));
        sb.AppendLine();

        sb.AppendLine("LEITURAS DETECTADAS:");
        foreach (var grupo in leituras.GroupBy(x => $"{x.TipoHardware} — {x.Hardware}").OrderBy(g => g.Key))
        {
            sb.AppendLine();
            sb.AppendLine(grupo.Key);

            foreach (var sensor in grupo.OrderBy(x => x.TipoSensor).ThenBy(x => x.Sensor).Take(25))
            {
                string obs = EhLimiteTemperatura(sensor)
                    ? " | limite informado pelo hardware"
                    : "";

                sb.AppendLine($"- {sensor.Sensor}: {sensor.Valor:0.0} {sensor.Unidade} ({sensor.TipoSensor}){obs}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("REFERÊNCIA GERAL:");
        sb.AppendLine("- CPU/GPU abaixo de 75°C: geralmente OK.");
        sb.AppendLine("- 75–85°C: atenção.");
        sb.AppendLine("- Acima de 85°C: alto, pode causar queda de desempenho/travadas.");
        sb.AppendLine("- NVMe Composite abaixo de 60°C geralmente está OK.");
        sb.AppendLine("- Se Temperature #2 do NVMe ficar perto de 70°C+, vale melhorar airflow/dissipador.");
        sb.AppendLine();
        sb.AppendLine("SEGURANÇA:");
        sb.AppendLine("- Nenhuma alteração foi aplicada.");
        sb.AppendLine("- Nenhum processo foi fechado.");

        return sb.ToString();
    }

    private static string GerarResumoTemperaturas(List<SensorLeitura> leituras)
    {
        var tempsAtuais = leituras
            .Where(EhTemperaturaAtualValida)
            .ToList();

        var cargas = leituras
            .Where(x => x.TipoSensor == "Load")
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("RESUMO:");

        if (tempsAtuais.Count == 0)
        {
            sb.AppendLine("⚠️ Nenhuma temperatura atual válida foi exposta pelos sensores.");
        }
        else
        {
            var cpuMax = tempsAtuais
                .Where(x => x.TipoHardware.Contains("Cpu", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            var gpuMax = tempsAtuais
                .Where(x => x.TipoHardware.Contains("Gpu", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            var storageComposite = tempsAtuais
                .Where(x => x.TipoHardware.Contains("Storage", StringComparison.OrdinalIgnoreCase))
                .Where(x => x.Sensor.Contains("Composite", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            var storageOutro = tempsAtuais
                .Where(x => x.TipoHardware.Contains("Storage", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();

            if (cpuMax is not null)
                sb.AppendLine($"CPU: {cpuMax.Valor:0.0} °C — {ClassificarTemperatura(cpuMax.Valor, "CPU")}");

            if (gpuMax is not null)
                sb.AppendLine($"GPU: {gpuMax.Valor:0.0} °C — {ClassificarTemperatura(gpuMax.Valor, "GPU")}");

            if (storageComposite is not null)
                sb.AppendLine($"SSD/NVMe Composite: {storageComposite.Valor:0.0} °C — {ClassificarTemperatura(storageComposite.Valor, "Storage")}");

            if (storageOutro is not null && storageComposite is not null && storageOutro.Sensor != storageComposite.Sensor)
                sb.AppendLine($"SSD/NVMe sensor mais quente: {storageOutro.Valor:0.0} °C em {storageOutro.Sensor} — {ClassificarTemperatura(storageOutro.Valor, "StorageHot")}");

            var tempMax = tempsAtuais.OrderByDescending(x => x.Valor).First();
            sb.AppendLine($"Maior temperatura atual válida: {tempMax.Valor:0.0} °C em {tempMax.Hardware} / {tempMax.Sensor}");
        }

        sb.AppendLine();

        var cpuLoad = cargas
            .Where(x => x.TipoHardware.Contains("Cpu", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Valor)
            .FirstOrDefault();

        var gpuLoad = cargas
            .Where(x => x.TipoHardware.Contains("Gpu", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Valor)
            .FirstOrDefault();

        var memoriaLoad = cargas
            .Where(x => x.TipoHardware.Contains("Memory", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Valor)
            .FirstOrDefault();

        if (cpuLoad is not null)
            sb.AppendLine($"Uso CPU: {cpuLoad.Valor:0.0}%");

        if (gpuLoad is not null)
            sb.AppendLine($"Uso GPU: {gpuLoad.Valor:0.0}%");

        if (memoriaLoad is not null)
            sb.AppendLine($"Uso RAM: {memoriaLoad.Valor:0.0}%");

        return sb.ToString();
    }

    private static bool EhTemperaturaAtualValida(SensorLeitura s)
    {
        if (s.TipoSensor != "Temperature")
            return false;

        if (s.Valor <= 0)
            return false;

        if (EhLimiteTemperatura(s))
            return false;

        return true;
    }

    private static bool EhLimiteTemperatura(SensorLeitura s)
    {
        return s.Sensor.Contains("Critical", StringComparison.OrdinalIgnoreCase) ||
               s.Sensor.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
               s.Sensor.Contains("Limit", StringComparison.OrdinalIgnoreCase) ||
               s.Sensor.Contains("TjMax", StringComparison.OrdinalIgnoreCase);
    }

    private static string ClassificarTemperatura(float valor, string tipo)
    {
        if (tipo == "Storage")
        {
            if (valor < 60) return "🟢 OK";
            if (valor < 70) return "🟡 Atenção";
            return "🔴 Alto";
        }

        if (tipo == "StorageHot")
        {
            if (valor < 65) return "🟢 OK";
            if (valor < 75) return "🟡 Atenção";
            return "🔴 Alto";
        }

        if (valor < 75) return "🟢 OK";
        if (valor < 85) return "🟡 Atenção";
        return "🔴 Alto";
    }

    private sealed record SensorLeitura(
        string Hardware,
        string TipoHardware,
        string Sensor,
        string TipoSensor,
        float Valor,
        string Unidade
    );
}













