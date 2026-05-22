using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ExilladosWarPrep;

public partial class MainWindow : Window
{
    private string _ultimoRelatorio = "Exillados WarPrep v0.2 pronto. Nenhum diagnóstico executado ainda.";

    public MainWindow()
    {
        InitializeComponent();
        RelatorioBox.Text = _ultimoRelatorio;
    }

    private async void OnDiagnosticarClick(object? sender, RoutedEventArgs e)
    {
        var modo = ObterModoTeste();

        StatusTitle.Text = "Diagnóstico em andamento...";
        ResumoText.Text = $"Executando exame {modo.Nome}. Não feche o programa durante o teste.";
        SistemaText.Text = "Coletando informações do sistema...";
        RedeText.Text = "Iniciando testes de rede...";
        TracertBox.Text = "Tracert ainda não executado...";
        RelatorioBox.Text = "Diagnóstico em andamento...";

        string powerPlan = await RunCommandAsync("powercfg", "/getactivescheme");
        string gameMode = LerModoJogo();
        string processos = ListarProcessosClassificados();

        var gateway = ObterGatewayPadrao();

        RedeText.Text = "Testando gateway/roteador...";
        PingResultado? pingGateway = null;

        if (!string.IsNullOrWhiteSpace(gateway))
            pingGateway = await TestarPingAsync("Gateway/Roteador", gateway, modo.Pacotes, 1200);

        RedeText.Text = "Testando Cloudflare 1.1.1.1...";
        var pingCloudflare = await TestarPingAsync("Cloudflare", "1.1.1.1", modo.Pacotes, 1200);

        RedeText.Text = "Testando Google DNS 8.8.8.8...";
        var pingGoogle = await TestarPingAsync("Google DNS", "8.8.8.8", modo.Pacotes, 1200);

        RedeText.Text = "Executando tracert...";
        string tracertCloudflare = await RunCommandAsync("tracert", $"-d -h {modo.TracertSaltos} 1.1.1.1");

        string diagnostico = GerarDiagnostico(pingGateway, pingCloudflare, pingGoogle);

        SistemaText.Text =
            $"Máquina: {Environment.MachineName}\n" +
            $"Sistema: {Environment.OSVersion}\n" +
            $"CPU lógica: {Environment.ProcessorCount}\n" +
            $"Modo Jogo: {gameMode}\n\n" +
            $"Plano de energia:\n{powerPlan}\n\n" +
            $"Processos:\n{processos}";

        var redeTela = new StringBuilder();

        redeTela.AppendLine($"Modo do exame: {modo.Nome}");
        redeTela.AppendLine($"Pacotes por alvo: {modo.Pacotes}");
        redeTela.AppendLine();

        if (pingGateway is not null)
            redeTela.AppendLine(pingGateway.ToDisplayString());
        else
            redeTela.AppendLine("Gateway/Roteador: não identificado.");

        redeTela.AppendLine();
        redeTela.AppendLine(pingCloudflare.ToDisplayString());
        redeTela.AppendLine();
        redeTela.AppendLine(pingGoogle.ToDisplayString());
        redeTela.AppendLine();
        redeTela.AppendLine("Diagnóstico:");
        redeTela.AppendLine(diagnostico);

        RedeText.Text = redeTela.ToString();
        TracertBox.Text = tracertCloudflare;

        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v0.2 =====\n" +
            $"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\n" +
            "OBJETIVO:\n" +
            "Ferramenta gratuita e open-source para ajudar players a prepararem o PC antes da Node War.\n" +
            "Não mexe no Black Desert Online, não é cheat e não coleta dados pessoais.\n\n" +
            "MODO DO EXAME:\n" +
            $"{modo.Nome} | Pacotes por alvo: {modo.Pacotes} | Tracert saltos: {modo.TracertSaltos}\n\n" +
            "SISTEMA:\n" +
            $"Máquina: {Environment.MachineName}\n" +
            $"Sistema: {Environment.OSVersion}\n" +
            $"CPU lógica: {Environment.ProcessorCount}\n" +
            $"Modo Jogo: {gameMode}\n\n" +
            "PLANO DE ENERGIA:\n" +
            $"{powerPlan}\n\n" +
            "REDE:\n" +
            $"{(pingGateway is not null ? pingGateway.ToReportString() : "Gateway/Roteador: não identificado.")}\n" +
            $"{pingCloudflare.ToReportString()}\n" +
            $"{pingGoogle.ToReportString()}\n\n" +
            "DIAGNÓSTICO:\n" +
            $"{diagnostico}\n\n" +
            "PROCESSOS:\n" +
            $"{processos}\n\n" +
            "TRACERT 1.1.1.1:\n" +
            $"{tracertCloudflare}\n\n" +
            "RECOMENDAÇÃO:\n" +
            "Se houver perda de pacote, ping muito alto ou falhas no teste, reinicie modem/roteador e teste novamente.\n" +
            "Antes da guerra, feche navegador pesado, launchers desnecessários e programas de atualização.\n" +
            "O programa não recomenda fechar BlackDesert64, Discord, antivírus, drivers ou serviços essenciais.\n";

        RelatorioBox.Text = _ultimoRelatorio;
        StatusTitle.Text = "Diagnóstico concluído";
        ResumoText.Text = "Relatório gerado. Você já pode copiar e mandar no Discord.";
    }

    private void OnPrepararClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Modo Guerra ainda em modo seguro";
        ResumoText.Text = "Nesta versão v0.2, o botão ainda não altera o Windows. Primeiro vamos validar diagnóstico, rede e relatório.";
        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v0.2 =====\n\n" +
            "Modo Preparar para Guerra ainda está em modo seguro.\n" +
            "Nenhuma alteração foi aplicada no Windows.\n\n" +
            "Próximas versões poderão aplicar, com confirmação e reversão:\n" +
            "- Ativar Modo Jogo\n" +
            "- Alterar plano de energia\n" +
            "- Limpar temporários seguros\n" +
            "- Fechar apps escolhidos pelo jogador\n" +
            "- Restaurar configurações anteriores\n";
        RelatorioBox.Text = _ultimoRelatorio;
    }

    private void OnRestaurarClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Nada para restaurar ainda";
        ResumoText.Text = "A versão v0.2 ainda não altera configurações, então não há nada para reverter.";
        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v0.2 =====\n\n" +
            "Restaurar Configurações:\n" +
            "Nenhuma alteração foi feita ainda pelo programa.\n";
        RelatorioBox.Text = _ultimoRelatorio;
    }

    private async void OnCopiarRelatorioClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is null)
        {
            StatusTitle.Text = "Não foi possível acessar a área de transferência";
            return;
        }

        await topLevel.Clipboard.SetTextAsync(_ultimoRelatorio);
        StatusTitle.Text = "Relatório copiado";
        ResumoText.Text = "Agora é só colar no Discord.";
    }

    private ModoTeste ObterModoTeste()
    {
        return ModoTesteCombo.SelectedIndex switch
        {
            1 => new ModoTeste("Médio — recomendado", 20, 15),
            2 => new ModoTeste("Longo — diagnóstico detalhado", 60, 30),
            _ => new ModoTeste("Curto — rápido antes da Node", 5, 8)
        };
    }

    private static async Task<PingResultado> TestarPingAsync(string nome, string host, int pacotes, int timeoutMs)
    {
        var tempos = new List<long>();
        int falhas = 0;

        using var ping = new Ping();

        for (int i = 0; i < pacotes; i++)
        {
            try
            {
                var resposta = await ping.SendPingAsync(host, timeoutMs);

                if (resposta.Status == IPStatus.Success)
                    tempos.Add(resposta.RoundtripTime);
                else
                    falhas++;
            }
            catch
            {
                falhas++;
            }
        }

        if (tempos.Count == 0)
            return new PingResultado(nome, host, pacotes, falhas, null, null, null);

        return new PingResultado(
            nome,
            host,
            pacotes,
            falhas,
            tempos.Average(),
            tempos.Min(),
            tempos.Max()
        );
    }

    private static string GerarDiagnostico(PingResultado? gateway, PingResultado cloudflare, PingResultado google)
    {
        var problemas = new List<string>();

        if (gateway is not null)
        {
            if (gateway.PerdaPercentual > 0)
                problemas.Add("❌ Perda de pacote até o roteador. Forte indício de problema local: Wi-Fi, cabo, placa de rede, modem/roteador ou rede interna.");

            if (gateway.MaximoMs >= 20)
                problemas.Add("⚠️ Pico alto até o roteador. Pode indicar instabilidade local.");
        }

        if (cloudflare.PerdaPercentual > 0 || google.PerdaPercentual > 0)
            problemas.Add("⚠️ Perda de pacote detectada fora da rede local. Pode ser rota, operadora ou instabilidade momentânea.");

        if ((cloudflare.MaximoMs ?? 0) >= 120 || (google.MaximoMs ?? 0) >= 120)
            problemas.Add("⚠️ Pico de ping alto detectado. Isso pode causar travadas/teleporte/atraso em guerra.");

        if ((cloudflare.MediaMs ?? 0) >= 100 || (google.MediaMs ?? 0) >= 100)
            problemas.Add("⚠️ Média de ping alta para os alvos testados.");

        if (problemas.Count == 0)
            return "✅ Rede aparentemente OK neste exame. Sem perda de pacote e sem pico relevante detectado.";

        return string.Join("\n", problemas) + "\n\nRecomendação: reinicie modem/roteador, teste via cabo se possível e repita o exame. Se persistir, pode ser caso para acionar a operadora.";
    }

    private static string? ObterGatewayPadrao()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().GatewayAddresses)
                .Select(g => g.Address)
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .FirstOrDefault(ip => !string.IsNullOrWhiteSpace(ip) && ip != "0.0.0.0");
        }
        catch
        {
            return null;
        }
    }

    private static string ListarProcessosClassificados()
    {
        try
        {
            var processos = Process.GetProcesses()
                .Select(p => new ProcessoInfo(p.ProcessName, ObterMemoriaMb(p)))
                .Where(p => p.MemoriaMb > 0)
                .OrderByDescending(p => p.MemoriaMb)
                .Take(12)
                .Select(FormatarProcesso);

            return string.Join("\n", processos);
        }
        catch
        {
            return "Não foi possível listar processos.";
        }
    }

    private static string FormatarProcesso(ProcessoInfo processo)
    {
        string nome = processo.Nome.ToLowerInvariant();

        if (nome.Contains("blackdesert"))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Jogo detectado — NÃO recomendar fechar.";

        if (nome.Contains("discord"))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Comunicação da guild — não fechar automaticamente.";

        if (nome.Contains("chrome") || nome.Contains("msedge") || nome.Contains("firefox") || nome.Contains("brave"))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Navegador pesado — pode fechar antes da guerra se não precisar.";

        if (nome.Contains("steam") || nome.Contains("epic") || nome.Contains("onedrive") || nome.Contains("teams") || nome.Contains("torrent"))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Pode atrapalhar — avaliar fechar antes da guerra.";

        return $"- {processo.Nome}: {processo.MemoriaMb} MB";
    }

    private static long ObterMemoriaMb(Process processo)
    {
        try
        {
            return processo.WorkingSet64 / 1024 / 1024;
        }
        catch
        {
            return 0;
        }
    }

    private static string LerModoJogo()
    {
        if (!OperatingSystem.IsWindows())
            return "Não disponível fora do Windows";

        return LerModoJogoWindows();
    }

    [SupportedOSPlatform("windows")]
    private static string LerModoJogoWindows()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\GameBar");
            var valor = key?.GetValue("AutoGameModeEnabled");

            return valor?.ToString() switch
            {
                "1" => "Ativado",
                "0" => "Desativado",
                _ => "Não identificado"
            };
        }
        catch
        {
            return "Não identificado";
        }
    }

    private static async Task<string> RunCommandAsync(string arquivo, string argumentos)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = arquivo,
                Arguments = argumentos,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processo = Process.Start(startInfo);

            if (processo is null)
                return "Não foi possível executar o comando.";

            string saida = await processo.StandardOutput.ReadToEndAsync();
            string erro = await processo.StandardError.ReadToEndAsync();

            await processo.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(saida))
                return saida.Trim();

            if (!string.IsNullOrWhiteSpace(erro))
                return erro.Trim();

            return "Comando executado sem saída.";
        }
        catch (Exception ex)
        {
            return $"Erro ao executar comando: {ex.Message}";
        }
    }

    private sealed record ModoTeste(string Nome, int Pacotes, int TracertSaltos);

    private sealed record ProcessoInfo(string Nome, long MemoriaMb);

    private sealed record PingResultado(
        string Nome,
        string Host,
        int Pacotes,
        int Falhas,
        double? MediaMs,
        long? MinimoMs,
        long? MaximoMs)
    {
        public double PerdaPercentual => Pacotes <= 0 ? 0 : (Falhas / (double)Pacotes) * 100.0;

        public string ToDisplayString()
        {
            if (MediaMs is null)
                return $"{Nome} {Host}:\nFalhou em todos os testes. Perda: 100%.";

            return $"{Nome} {Host}:\nMédia: {MediaMs:0.0} ms | Mín: {MinimoMs} ms | Máx: {MaximoMs} ms | Perda: {PerdaPercentual:0}%";
        }

        public string ToReportString()
        {
            if (MediaMs is null)
                return $"{Nome} {Host}: falhou em todos os testes | Perda: 100%";

            return $"{Nome} {Host}: Média: {MediaMs:0.0} ms | Mín: {MinimoMs} ms | Máx: {MaximoMs} ms | Perda: {PerdaPercentual:0}%";
        }
    }
}
