using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ExilladosWarPrep;

public partial class MainWindow : Window
{
    private string _ultimoRelatorio = "Exillados WarPrep v3.0 pronto. Nenhum diagnóstico executado ainda.";
    private bool _repararWindowsConfirmacaoPendente = false;
    private bool _limpezaConfirmacaoPendente = false;
    private bool _fecharSelecionadosConfirmacaoPendente = false;
    private bool _modoJogoConfirmacaoPendente = false;
    private bool _energiaConfirmacaoPendente = false;
    private bool _restaurarEnergiaConfirmacaoPendente = false;

    public MainWindow()
    {
        InitializeComponent();
        RelatorioBox.Text = _ultimoRelatorio;
    }




    private async void OnProcessosSuspeitosClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Triagem de processos iniciada";
        ResumoText.Text = "Analisando caminhos, assinaturas digitais e processos com comportamento suspeito.";
        SistemaText.Text = "Coletando processos...";
        RelatorioBox.Text = "Triagem de processos suspeitos em andamento...";

        if (!OperatingSystem.IsWindows())
        {
            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
                "TRIAGEM DE PROCESSOS SUSPEITOS:\n\n" +
                "Esta função só está disponível no Windows.\n" +
                "Nenhuma alteração foi aplicada.\n";

            SistemaText.Text = _ultimoRelatorio;
            RelatorioBox.Text = _ultimoRelatorio;
            return;
        }

        string script = """
$ErrorActionPreference = 'SilentlyContinue'

$systemNames = @(
  'svchost',
  'lsass',
  'winlogon',
  'csrss',
  'services',
  'smss',
  'explorer',
  'dwm',
  'spoolsv'
)

$trustedNoise = @(
  'discord',
  'chrome',
  'msedge',
  'devenv',
  'dotnet',
  'exilladoswarprep',
  'steam',
  'nvidia',
  'amd',
  'kaspersky',
  'avp',
  'blackdesert',
  'coherentui_host'
)

$win = $env:windir.ToLower()
$resultados = @()

Get-Process | Where-Object { $_.Path } | Select-Object -First 180 | ForEach-Object {
    $nome = $_.ProcessName
    $id = $_.Id
    $path = $_.Path
    $lower = $path.ToLower()
    $nlower = $nome.ToLower()
    $motivos = New-Object System.Collections.Generic.List[string]

    if ($lower -like '*\appdata\local\temp\*') {
        $motivos.Add('rodando da pasta Temp')
    }

    if ($lower -like '*\downloads\*') {
        $motivos.Add('rodando da pasta Downloads')
    }

    if ($lower -like '*\appdata\roaming\*') {
        $ehRuido = $false
        foreach ($r in $trustedNoise) {
            if ($lower -like "*$r*") { $ehRuido = $true }
        }

        if (-not $ehRuido) {
            $motivos.Add('rodando de AppData\Roaming')
        }
    }

    if ($systemNames -contains $nlower) {
        if (-not $lower.StartsWith($win)) {
            $motivos.Add('nome parecido com processo do Windows fora da pasta Windows')
        }
    }

    $sigStatus = 'Não verificado'
    $sigSigner = ''

    try {
        $sig = Get-AuthenticodeSignature -FilePath $path
        $sigStatus = $sig.Status.ToString()

        if ($sig.SignerCertificate) {
            $sigSigner = $sig.SignerCertificate.Subject
        }

        if ($sigStatus -ne 'Valid') {
            $ehRuido = $false
            foreach ($r in $trustedNoise) {
                if ($lower -like "*$r*") { $ehRuido = $true }
            }

            if (-not $ehRuido) {
                $motivos.Add("assinatura digital: $sigStatus")
            }
        }
    }
    catch {
        $sigStatus = 'Erro ao verificar'
    }

    if ($motivos.Count -gt 0) {
        $resultados += [PSCustomObject]@{
            Processo = $nome
            PID = $id
            Motivo = ($motivos -join '; ')
            Assinatura = $sigStatus
            Assinante = $sigSigner
            Caminho = $path
        }
    }
}

if ($resultados.Count -eq 0) {
    'Nenhum processo suspeito simples foi detectado nesta triagem.'
}
else {
    $resultados | Select-Object -First 30 | Format-List | Out-String
}
""";

        string saida = await RunPowerShellAsync(script);

        var sb = new StringBuilder();

        sb.AppendLine("===== EXILLADOS WARPREP v3.0 =====");
        sb.AppendLine();
        sb.AppendLine("🕵️ TRIAGEM DE PROCESSOS SUSPEITOS");
        sb.AppendLine();
        sb.AppendLine("IMPORTANTE:");
        sb.AppendLine("- O WarPrep NÃO é antivírus.");
        sb.AppendLine("- Esta triagem apenas aponta sinais suspeitos simples.");
        sb.AppendLine("- Nada foi removido, fechado ou alterado.");
        sb.AppendLine("- Para ameaça real, use antivírus confiável e análise manual.");
        sb.AppendLine();

        sb.AppendLine("O QUE FOI VERIFICADO:");
        sb.AppendLine("- Processo rodando de Temp/Downloads/AppData suspeito.");
        sb.AppendLine("- Nome parecido com processo do Windows fora da pasta Windows.");
        sb.AppendLine("- Assinatura digital inválida/ausente em itens suspeitos.");
        sb.AppendLine();

        sb.AppendLine("RESULTADO:");
        sb.AppendLine(LimparSaida(saida));
        sb.AppendLine();

        sb.AppendLine("SEGURANÇA:");
        sb.AppendLine("- Nenhum processo foi fechado.");
        sb.AppendLine("- Nenhum arquivo foi apagado.");
        sb.AppendLine("- Nenhuma configuração do Windows foi alterada.");

        _ultimoRelatorio = sb.ToString();

        StatusTitle.Text = "Triagem de processos concluída";
        ResumoText.Text = "Relatório de processos suspeitos gerado. Use Copiar ou Salvar Relatório.";
        SistemaText.Text = _ultimoRelatorio;
        RelatorioBox.Text = _ultimoRelatorio;
    }


    private void OnTemperaturasClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Lendo sensores";
        ResumoText.Text = "Coletando temperatura e uso de CPU/GPU/SSD.";
        SistemaText.Text = "Lendo sensores pelo LibreHardwareMonitor...";
        RelatorioBox.Text = "Leitura de sensores em andamento...";

        var leituras = ColetarSensoresHardware();
        _ultimoRelatorio = GerarRelatorioTemperaturas(leituras);

        StatusTitle.Text = "Temperaturas lidas";
        ResumoText.Text = "Relatório de sensores gerado.";
        SistemaText.Text = _ultimoRelatorio;
        RelatorioBox.Text = _ultimoRelatorio;
    }
    private async void OnDiagnosticoGamerClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Diagnóstico gamer em andamento...";
        ResumoText.Text = "Coletando hardware, sistema, rede local, memória, disco e processos.";
        SistemaText.Text = "Coletando dados do PC...";
        RelatorioBox.Text = "Diagnóstico gamer em andamento...";

        string windows = await RunPowerShellAsync("$os = Get-CimInstance Win32_OperatingSystem; $os.Caption + ' | Build ' + $os.BuildNumber + ' | ' + $os.OSArchitecture");
        string cpu = await RunPowerShellAsync("(Get-CimInstance Win32_Processor | Select-Object -First 1).Name");
        string gpu = await RunPowerShellAsync("Get-CimInstance Win32_VideoController | ForEach-Object { $_.Name }");
        string ram = await RunPowerShellAsync("$os = Get-CimInstance Win32_OperatingSystem; 'Total: ' + [math]::Round($os.TotalVisibleMemorySize/1MB,1) + ' GB | Livre: ' + [math]::Round($os.FreePhysicalMemory/1MB,1) + ' GB'");
        string placaMae = await RunPowerShellAsync("Get-CimInstance Win32_BaseBoard | ForEach-Object { $_.Manufacturer + ' ' + $_.Product }");
        string bios = await RunPowerShellAsync("Get-CimInstance Win32_BIOS | ForEach-Object { $_.Manufacturer + ' | ' + $_.SMBIOSBIOSVersion }");
        string discos = await RunPowerShellAsync("Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' | ForEach-Object { $_.DeviceID + ' Livre: ' + [math]::Round($_.FreeSpace/1GB,1) + ' GB / Total: ' + [math]::Round($_.Size/1GB,1) + ' GB' }");
        string rede = await RunPowerShellAsync("Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 6 Name, InterfaceDescription, LinkSpeed | Format-Table -AutoSize | Out-String");
        string ipInfo = await RunPowerShellAsync("Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway -ne $null } | Select-Object -First 3 InterfaceAlias, IPv4Address, IPv4DefaultGateway, DNSServer | Format-List | Out-String");

        string gameMode = LerModoJogo();
        string powerPlan = await RunCommandAsync("powercfg", "/getactivescheme");

        var processos = Process.GetProcesses()
            .Select(p => new ProcessoInfo(p.ProcessName, ObterMemoriaMb(p)))
            .Where(p => p.MemoriaMb > 0)
            .GroupBy(p => p.Nome, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProcessoInfo(g.Key, g.Sum(x => x.MemoriaMb)))
            .OrderByDescending(p => p.MemoriaMb)
            .Take(15)
            .ToList();

        bool bdo = processos.Any(p => EhBlackDesert(p.Nome));
        bool discord = processos.Any(p => EhDiscord(p.Nome));

        var candidatos = processos
            .Where(p => EhCandidatoParaFechar(p.Nome))
            .OrderByDescending(p => p.MemoriaMb)
            .Take(8)
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("===== EXILLADOS WARPREP v3.0 =====");
        sb.AppendLine();
        sb.AppendLine("🖥️ DIAGNÓSTICO GAMER DO PC");
        sb.AppendLine();

        sb.AppendLine("SISTEMA:");
        sb.AppendLine($"Windows: {LimparSaida(windows)}");
        sb.AppendLine($"Modo Jogo: {gameMode}");
        sb.AppendLine("Plano de energia:");
        sb.AppendLine(LimparSaida(powerPlan));
        sb.AppendLine();

        sb.AppendLine("HARDWARE:");
        sb.AppendLine($"CPU: {LimparSaida(cpu)}");
        sb.AppendLine("GPU:");
        sb.AppendLine(LimparSaida(gpu));
        sb.AppendLine($"RAM: {LimparSaida(ram)}");
        sb.AppendLine($"Placa-mãe: {LimparSaida(placaMae)}");
        sb.AppendLine($"BIOS: {LimparSaida(bios)}");
        sb.AppendLine();

        sb.AppendLine("DISCOS:");
        sb.AppendLine(LimparSaida(discos));
        sb.AppendLine();

        sb.AppendLine("REDE LOCAL:");
        sb.AppendLine(LimparSaida(rede));
        sb.AppendLine("IP / Gateway / DNS:");
        sb.AppendLine(LimparSaida(ipInfo));
        sb.AppendLine();

        sb.AppendLine("JOGO E COMUNICAÇÃO:");
        sb.AppendLine(bdo ? "✅ Black Desert detectado" : "⚠️ Black Desert não detectado aberto");
        sb.AppendLine(discord ? "✅ Discord detectado" : "⚠️ Discord não detectado aberto");
        sb.AppendLine();

        sb.AppendLine("PROCESSOS MAIS PESADOS:");
        foreach (var p in processos)
            sb.AppendLine($"- {p.Nome}: {p.MemoriaMb} MB");

        sb.AppendLine();
        sb.AppendLine("APPS QUE PODEM SER FECHADOS SE NÃO ESTIVER USANDO:");
        if (candidatos.Count == 0)
        {
            sb.AppendLine("- Nenhum candidato seguro detectado agora.");
        }
        else
        {
            foreach (var p in candidatos)
                sb.AppendLine($"- {p.Nome}: {p.MemoriaMb} MB — {MotivoPodeFechar(p.Nome)}");
        }

        sb.AppendLine();
        sb.AppendLine("TEMPERATURA / FPS:");
        sb.AppendLine("⚠️ Temperatura CPU/GPU ainda não integrada nesta versão.");
        sb.AppendLine("⚠️ FPS ainda não integrado nesta versão.");
        sb.AppendLine("Próximos módulos recomendados:");
        sb.AppendLine("- LibreHardwareMonitor para temperatura CPU/GPU/SSD.");
        sb.AppendLine("- PresentMon para FPS/frame time experimental.");
        sb.AppendLine();

        sb.AppendLine("SEGURANÇA:");
        sb.AppendLine("- Nenhuma alteração foi aplicada.");
        sb.AppendLine("- Nenhum processo foi fechado.");
        sb.AppendLine("- Este diagnóstico apenas lê informações do sistema.");

        _ultimoRelatorio = sb.ToString();

        StatusTitle.Text = "Diagnóstico gamer concluído";
        ResumoText.Text = "Hardware, rede local, discos, sistema e processos foram analisados.";
        SistemaText.Text = _ultimoRelatorio;
        RelatorioBox.Text = _ultimoRelatorio;
    }

    private static async Task<string> RunPowerShellAsync(string comando)
    {
        try
        {
            var bytes = Encoding.Unicode.GetBytes(comando);
            string encoded = Convert.ToBase64String(bytes);
            return await RunCommandAsync("powershell", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}");
        }
        catch (Exception ex)
        {
            return $"Erro ao executar PowerShell: {ex.Message}";
        }
    }

    private static string LimparSaida(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return "Não identificado";

        return texto.Trim();
    }
    private async void OnDiagnosticarClick(object? sender, RoutedEventArgs e)
    {
        var modo = ObterModoTeste();

        int totalAlvos = 3;
        int totalPacotes = modo.Pacotes * totalAlvos;
        int pacotesConcluidos = 0;

        ResetarPainelAoVivo();

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
        PingResultado? pingGateway = null;

        if (!string.IsNullOrWhiteSpace(gateway))
        {
            pingGateway = await TestarPingAsync(
                "Gateway/Roteador",
                gateway,
                modo.Pacotes,
                1200,
                totalPacotes,
                () => pacotesConcluidos++,
                AtualizarPainelAoVivo);
        }
        else
        {
            pacotesConcluidos += modo.Pacotes;
            AtualizarPainelManual("Gateway/Roteador", modo.Pacotes, modo.Pacotes, null, "Gateway não identificado.", pacotesConcluidos, totalPacotes);
        }

        var pingCloudflare = await TestarPingAsync(
            "Cloudflare",
            "1.1.1.1",
            modo.Pacotes,
            1200,
            totalPacotes,
            () => pacotesConcluidos++,
            AtualizarPainelAoVivo);

        var pingGoogle = await TestarPingAsync(
            "Google DNS",
            "8.8.8.8",
            modo.Pacotes,
            1200,
            totalPacotes,
            () => pacotesConcluidos++,
            AtualizarPainelAoVivo);

        EstadoAtualText.Text = "Executando tracert";
        AlvoAtualText.Text = "Tracert 1.1.1.1";
        PingAtualText.Text = "-- ms";
        ResumoAoVivoText.Text = "Ping finalizado. Executando tracert curto da rota...";
        ProgressoBar.Value = 100;

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

        AtualizarMedidor(pingCloudflare.MediaMs is null ? null : (long)Math.Round(pingCloudflare.MediaMs.Value), "Exame concluído");

        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v3.0 =====\n" +
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
        EstadoAtualText.Text = "Concluído";
        AlvoAtualText.Text = "Exame finalizado";
        PacoteAtualText.Text = $"{totalPacotes}/{totalPacotes}";
        ResumoAoVivoText.Text = diagnostico;
        ProgressoBar.Value = 100;
    }

    private void ResetarPainelAoVivo()
    {
        AlvoAtualText.Text = "Iniciando";
        PacoteAtualText.Text = "0/0";
        PingAtualText.Text = "-- ms";
        EstadoAtualText.Text = "Preparando";
        ResumoAoVivoText.Text = "Preparando exame de rede...";
        ProgressoBar.Value = 0;
        AtualizarMedidor(null, "Aguardando exame");
    }

    private void AtualizarPainelAoVivo(string alvo, int pacoteAtualDoAlvo, int pacotesDoAlvo, long? pingAtualMs, string estado, int pacotesConcluidos, int totalPacotes)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AlvoAtualText.Text = alvo;
            PacoteAtualText.Text = $"{pacoteAtualDoAlvo}/{pacotesDoAlvo}";
            PingAtualText.Text = pingAtualMs is null ? "falhou" : $"{pingAtualMs} ms";
            EstadoAtualText.Text = estado;

            double progresso = totalPacotes <= 0 ? 0 : (pacotesConcluidos / (double)totalPacotes) * 100.0;
            ProgressoBar.Value = Math.Max(0, Math.Min(100, progresso));

            ResumoAoVivoText.Text =
                $"Testando {alvo}. Pacote {pacoteAtualDoAlvo}/{pacotesDoAlvo}. " +
                $"Progresso geral: {progresso:0}%";

            AtualizarMedidor(pingAtualMs, estado);
        });
    }

    private void AtualizarPainelManual(string alvo, int pacoteAtualDoAlvo, int pacotesDoAlvo, long? pingAtualMs, string estado, int pacotesConcluidos, int totalPacotes)
    {
        AlvoAtualText.Text = alvo;
        PacoteAtualText.Text = $"{pacoteAtualDoAlvo}/{pacotesDoAlvo}";
        PingAtualText.Text = pingAtualMs is null ? "-- ms" : $"{pingAtualMs} ms";
        EstadoAtualText.Text = estado;

        double progresso = totalPacotes <= 0 ? 0 : (pacotesConcluidos / (double)totalPacotes) * 100.0;
        ProgressoBar.Value = Math.Max(0, Math.Min(100, progresso));
        ResumoAoVivoText.Text = estado;
        AtualizarMedidor(pingAtualMs, estado);
    }

    private void AtualizarMedidor(long? pingAtualMs, string estado)
    {
        if (pingAtualMs is null)
        {
            GaugePingText.Text = "-- ms";
            GaugeStatusText.Text = estado;
            GaugeHintText.Text = "Aguardando ping para calcular a qualidade.";
            GaugeQualityBar.Value = 0;
            GaugeQualityBar.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#64748B"));
            return;
        }

        double ping = pingAtualMs.Value;
        double qualidade = Math.Max(0, Math.Min(100, 100 - (ping / 2.0)));

        GaugePingText.Text = $"{pingAtualMs.Value} ms";
        GaugeQualityBar.Value = qualidade;
        GaugeHintText.Text = $"Qualidade aproximada: {qualidade:0}%";

        if (ping < 30)
        {
            GaugeStatusText.Text = "Excelente";
            GaugeQualityBar.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#22C55E"));
        }
        else if (ping < 70)
        {
            GaugeStatusText.Text = "OK";
            GaugeQualityBar.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#22C55E"));
        }
        else if (ping < 120)
        {
            GaugeStatusText.Text = "Atenção";
            GaugeQualityBar.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#EAB308"));
        }
        else
        {
            GaugeStatusText.Text = "Ruim / pico alto";
            GaugeQualityBar.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#EF4444"));
        }
    }






    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("windows")]
    private ModoTeste ObterModoTeste()
    {
        return ModoTesteCombo.SelectedIndex switch
        {
            1 => new ModoTeste("Médio — recomendado", 20, 15),
            2 => new ModoTeste("Longo — diagnóstico detalhado", 60, 30),
            _ => new ModoTeste("Curto — rápido antes da Node", 5, 8)
        };
    }

    private static async Task<PingResultado> TestarPingAsync(string nome, string host, int pacotes, int timeoutMs, int totalPacotes, Func<int> incrementarConcluidos, Action<string, int, int, long?, string, int, int> atualizarPainel)
    {
        var tempos = new List<long>();
        int falhas = 0;

        using var ping = new Ping();

        for (int i = 1; i <= pacotes; i++)
        {
            long? pingAtual = null;
            string estado = "OK";

            try
            {
                var resposta = await ping.SendPingAsync(host, timeoutMs);

                if (resposta.Status == IPStatus.Success)
                {
                    pingAtual = resposta.RoundtripTime;
                    tempos.Add(resposta.RoundtripTime);

                    if (resposta.RoundtripTime >= 120)
                        estado = "Pico alto";
                    else if (resposta.RoundtripTime >= 70)
                        estado = "Atenção";
                    else
                        estado = "OK";
                }
                else
                {
                    falhas++;
                    estado = "Falha";
                }
            }
            catch
            {
                falhas++;
                estado = "Falha";
            }

            int concluidos = incrementarConcluidos();
            atualizarPainel(nome, i, pacotes, pingAtual, estado, concluidos, totalPacotes);

            await Task.Delay(120);
        }

        if (tempos.Count == 0)
            return new PingResultado(nome, host, pacotes, falhas, null, null, null);

        return new PingResultado(nome, host, pacotes, falhas, tempos.Average(), tempos.Min(), tempos.Max());
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

        if (EhBlackDesert(processo.Nome))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Jogo detectado — NÃO recomendar fechar.";

        if (EhDiscord(processo.Nome))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Comunicação da guild — não fechar automaticamente.";

        if (EhNavegadorChecklist(processo.Nome))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Navegador pesado — pode fechar antes da guerra se não precisar.";

        if (EhAppQuePodeAtrapalharChecklist(processo.Nome))
            return $"- {processo.Nome}: {processo.MemoriaMb} MB | Pode atrapalhar — avaliar fechar antes da guerra.";

        return $"- {processo.Nome}: {processo.MemoriaMb} MB";
    }

    private static bool EhBlackDesert(string nome)
    {
        string n = nome.ToLowerInvariant();
        return n.Contains("blackdesert") || n.Contains("blackdesert64");
    }

    private static bool EhDiscord(string nome)
    {
        return nome.Contains("Discord", StringComparison.OrdinalIgnoreCase);
    }


    private static bool EhEdgeNormal(string n)
    {
        return n.Contains("msedge") && !n.Contains("msedgewebview2");
    }
    private static bool EhNavegadorChecklist(string nome)
    {
        string n = nome.ToLowerInvariant();
        return n.Contains("chrome") ||
               EhEdgeNormal(n) ||
               n.Contains("firefox") ||
               n.Contains("brave") ||
               n.Contains("opera");
    }


    private static bool EhEaApp(string n)
    {
        return n == "ea" ||
               n == "eaapp" ||
               n == "eadesktop" ||
               n == "ealink" ||
               n.Contains("electronicarts");
    }
    private static bool EhAppQuePodeAtrapalharChecklist(string nome)
    {
        string n = nome.ToLowerInvariant();
        return n.Contains("steam") ||
               n.Contains("epic") ||
               n.Contains("onedrive") ||
               n.Contains("teams") ||
               n.Contains("torrent") ||
               n.Contains("utorrent") ||
               n.Contains("bittorrent") ||
               n.Contains("dropbox") ||
               n.Contains("googledrive") ||
               EhEaApp(n) ||
               n.Contains("battle.net") ||
               n.Contains("riotclient");
    }

    private static bool EhProcessoProtegido(string nome)
    {
        string n = nome.ToLowerInvariant();

        return EhBlackDesert(nome) ||
               EhDiscord(nome) ||
               n.Contains("nvidia") ||
               n.Contains("amd") ||
               n.Contains("radeon") ||
               n.Contains("realtek") ||
               n.Contains("windowsdefender") ||
               n.Contains("securityhealth") ||
               n.Contains("msmpeng") ||
               n.Contains("explorer") ||
               n.Contains("dwm") ||
               n.Contains("audiodg") ||
               n.Contains("steamservice") ||
               n.Contains("searchhost") ||
               n.Contains("searchindexer") ||
               n.Contains("searchfilterhost") ||
               n.Contains("searchprotocolhost") ||
               n.Contains("exilladoswarprep") ||
               n.Contains("dotnet") ||
               n.Contains("vbcscompiler") ||
               n.Contains("devenv") ||
               n.Contains("devhub") ||
               n.Contains("coherentui_host") ||
               n.Contains("avp") ||
               n.Contains("kaspersky") ||
               n.Contains("nvcontainer") ||
               n.Contains("svchost") ||
               n.Contains("runtimebroker") ||
               n.Contains("startmenuexperiencehost") ||
               n.Contains("crossdeviceservice") ||
               n.Contains("powershell") ||
               n.Contains("pwsh") ||
               n.Contains("nvdisplay.container") ||
               n.Contains("secure system") ||
               n.Contains("registry") ||
               n.Contains("textinputhost") ||
               n.Contains("accountscontrolhost") ||
               n.Contains("perfwatson2") ||
               n.Contains("memory compression") ||
               n.Contains("msedgewebview2");
    }

    private static bool EhCandidatoParaFechar(string nome)
    {
        if (EhProcessoProtegido(nome))
            return false;

        return EhNavegadorChecklist(nome) || EhAppQuePodeAtrapalharChecklist(nome);
    }

    private static string MotivoProtegido(string nome)
    {
        string n = nome.ToLowerInvariant();

        if (EhBlackDesert(nome))
            return "jogo detectado, nunca fechar pelo WarPrep";

        if (EhDiscord(nome))
            return "comunicação da guild, não fechar automaticamente";

        if (n.Contains("nvidia") || n.Contains("nvcontainer") || n.Contains("nvdisplay.container") || n.Contains("amd") || n.Contains("radeon"))
            return "driver de vídeo, proteger";

        if (n.Contains("realtek") || n.Contains("audiodg"))
            return "áudio/driver, proteger";

        if (n.Contains("msmpeng") || n.Contains("securityhealth") || n.Contains("windowsdefender"))
            return "segurança do Windows/antivírus, proteger";

        if (n.Contains("explorer") || n.Contains("dwm"))
            return "processo essencial do Windows";

        if (n.Contains("searchhost") || n.Contains("searchindexer") || n.Contains("searchfilterhost") || n.Contains("searchprotocolhost"))
            return "serviço de busca/indexação do Windows, não fechar automaticamente";

        if (n.Contains("exilladoswarprep") || n.Contains("dotnet") || n.Contains("vbcscompiler") || n.Contains("devenv") || n.Contains("devhub"))
            return "ferramenta do próprio app/desenvolvimento, não fechar automaticamente";

        if (n.Contains("coherentui_host"))
            return "componente relacionado a interface de jogo/app, não fechar automaticamente";

        if (n.Contains("avp") || n.Contains("kaspersky"))
            return "antivírus/Kaspersky, proteger";

        if (n.Contains("svchost") || n.Contains("runtimebroker") || n.Contains("startmenuexperiencehost") || n.Contains("crossdeviceservice"))
            return "processo do Windows, não fechar automaticamente";

        if (n.Contains("powershell") || n.Contains("pwsh"))
            return "terminal usado para rodar/testar o app, proteger";

        if (n.Contains("secure system") || n.Contains("registry") || n.Contains("textinputhost") || n.Contains("accountscontrolhost") || n.Contains("memory compression"))
            return "componente interno do Windows, não fechar automaticamente";

        if (n.Contains("perfwatson2"))
            return "telemetria/diagnóstico do Visual Studio, não fechar automaticamente durante desenvolvimento";

        if (n.Contains("msedgewebview2"))
            return "WebView2 usado por apps/launchers, não fechar automaticamente";

        return "processo protegido";
    }

    private static string MotivoPodeFechar(string nome)
    {
        string n = nome.ToLowerInvariant();

        if (EhNavegadorChecklist(nome))
            return "navegador pode consumir RAM/CPU";

        if (n.Contains("onedrive") || n.Contains("dropbox") || n.Contains("googledrive"))
            return "sincronização em nuvem pode consumir disco/rede";

        if (n.Contains("steam") || n.Contains("epic") || EhEaApp(n) || n.Contains("battle.net") || n.Contains("riotclient"))
            return "launcher pode atualizar/consumir rede";

        if (n.Contains("teams"))
            return "app de reunião pode consumir memória/rede";

        if (n.Contains("torrent") || n.Contains("utorrent") || n.Contains("bittorrent"))
            return "torrent pode consumir muita rede";

        return "pode atrapalhar se não for necessário";
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
            string comando = $"chcp 65001>nul & \"{arquivo}\" {argumentos}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + comando,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
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


    private sealed record PingResultado(string Nome, string Host, int Pacotes, int Falhas, double? MediaMs, long? MinimoMs, long? MaximoMs)
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










































