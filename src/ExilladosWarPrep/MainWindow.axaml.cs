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
    private string _ultimoRelatorio = "Exillados WarPrep v2.5.1 pronto. Nenhum diagnóstico executado ainda.";
    private bool _repararWindowsConfirmacaoPendente = false;
    private bool _limpezaConfirmacaoPendente = false;
    private bool _fecharAppsConfirmacaoPendente = false;
    private bool _fecharSelecionadosConfirmacaoPendente = false;
    private bool _modoJogoConfirmacaoPendente = false;
    private bool _energiaConfirmacaoPendente = false;
    private bool _restaurarEnergiaConfirmacaoPendente = false;

    public MainWindow()
    {
        InitializeComponent();
        RelatorioBox.Text = _ultimoRelatorio;
    }




    private async void OnRepararWindowsClick(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            StatusTitle.Text = "Reparo indisponível";
            ResumoText.Text = "Esta função só está disponível no Windows.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "REPARAR WINDOWS:\n\n" +
                "Esta função só está disponível no Windows.\n" +
                "Nenhuma alteração foi aplicada.\n";

            SistemaText.Text = _ultimoRelatorio;
            RelatorioBox.Text = _ultimoRelatorio;
            return;
        }

        bool admin = EstaRodandoComoAdministrador();

        if (!admin)
        {
            _repararWindowsConfirmacaoPendente = false;

            StatusTitle.Text = "Administrador necessário";
            ResumoText.Text = "Para reparar o Windows, feche o WarPrep e abra como Administrador.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "REPARAR WINDOWS:\n\n" +
                "⚠️ O WarPrep não está executando como Administrador.\n\n" +
                "Para usar o reparo:\n" +
                "1. Feche o WarPrep.\n" +
                "2. Clique com botão direito no ExilladosWarPrep.exe.\n" +
                "3. Escolha 'Executar como administrador'.\n" +
                "4. Clique novamente em 🔧 Reparar Windows.\n\n" +
                "Nenhuma alteração foi aplicada.\n";

            SistemaText.Text = _ultimoRelatorio;
            RelatorioBox.Text = _ultimoRelatorio;
            return;
        }

        if (!_repararWindowsConfirmacaoPendente)
        {
            _repararWindowsConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Clique novamente em Reparar Windows para iniciar DISM RestoreHealth e SFC Scannow.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "CONFIRMAÇÃO — REPARAR WINDOWS:\n\n" +
                "NADA FOI REPARADO AINDA.\n\n" +
                "O diagnóstico anterior indicou que o repositório de componentes do Windows é reparável.\n\n" +
                "Se confirmar, o WarPrep vai executar:\n" +
                "1. DISM /Online /Cleanup-Image /RestoreHealth\n" +
                "2. sfc /scannow\n\n" +
                "AVISOS:\n" +
                "- Pode demorar vários minutos.\n" +
                "- Pode usar CPU/disco/rede.\n" +
                "- Não faça perto da Node War.\n" +
                "- O programa não reinicia o PC sozinho.\n" +
                "- Depois do reparo, o Windows pode recomendar reiniciar.\n\n" +
                "Para confirmar, clique novamente em:\n" +
                "🔧 Reparar Windows\n";

            SistemaText.Text = _ultimoRelatorio;
            RelatorioBox.Text = _ultimoRelatorio;
            return;
        }

        _repararWindowsConfirmacaoPendente = false;

        var sb = new StringBuilder();

        sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
        sb.AppendLine();
        sb.AppendLine("🔧 REPARO DO WINDOWS — DISM / SFC");
        sb.AppendLine();
        sb.AppendLine("Permissão: Administrador confirmado");
        sb.AppendLine("Modo: reparo real iniciado com confirmação dupla");
        sb.AppendLine();

        SistemaText.Text = sb.ToString();
        RelatorioBox.Text = sb.ToString();

        StatusTitle.Text = "Rodando DISM RestoreHealth";
        ResumoText.Text = "Reparando imagem do Windows. Isso pode demorar bastante. Não feche o programa.";

        string dismRestore = await RunCommandAsync("dism", "/Online /Cleanup-Image /RestoreHealth");

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("DISM /Online /Cleanup-Image /RestoreHealth");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(LimparSaida(dismRestore));
        sb.AppendLine();

        SistemaText.Text = sb.ToString();
        RelatorioBox.Text = sb.ToString();

        StatusTitle.Text = "Rodando SFC Scannow";
        ResumoText.Text = "Verificando e reparando arquivos do sistema. Isso também pode demorar.";

        string sfcScannow = await RunCommandAsync("sfc", "/scannow");

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("sfc /scannow");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(LimparSaida(sfcScannow));
        sb.AppendLine();

        string conclusao = GerarConclusaoReparoWindows(dismRestore, sfcScannow);

        sb.AppendLine("CONCLUSÃO:");
        sb.AppendLine(conclusao);
        sb.AppendLine();
        sb.AppendLine("RECOMENDAÇÃO:");
        sb.AppendLine("- Salve este relatório.");
        sb.AppendLine("- Reinicie o PC se o Windows/SFC/DISM indicar ou se o sistema continuar estranho.");
        sb.AppendLine("- Depois de reiniciar, rode 🩺 Verificar Windows novamente.");

        _ultimoRelatorio = sb.ToString();

        StatusTitle.Text = "Reparo do Windows concluído";
        ResumoText.Text = "DISM RestoreHealth e SFC Scannow terminaram. Salve o relatório.";
        SistemaText.Text = _ultimoRelatorio;
        RelatorioBox.Text = _ultimoRelatorio;
    }


    private static string GerarConclusaoReparoWindows(string dism, string sfc)
    {
        string texto = (dism + "\n" + sfc).ToLowerInvariant();

        bool dismOk =
            texto.Contains("the restore operation completed successfully") ||
            texto.Contains("a operação de restauração foi concluída com êxito") ||
            texto.Contains("a operacao de restauracao foi concluida com exito") ||
            texto.Contains("operação foi concluída com êxito") ||
            texto.Contains("operacao foi concluida com exito") ||
            texto.Contains("a operação foi concluída com êxito") ||
            texto.Contains("a operacao foi concluida com exito");

        bool sfcReparou =
            texto.Contains("windows resource protection found corrupt files and successfully repaired them") ||
            texto.Contains("proteção de recursos do windows encontrou arquivos corrompidos e os reparou com êxito") ||
            texto.Contains("protecao de recursos do windows encontrou arquivos corrompidos e os reparou com exito") ||
            texto.Contains("encontrou arquivos corrompidos e os reparou com êxito") ||
            texto.Contains("encontrou arquivos corrompidos e os reparou com exito") ||
            texto.Contains("encontrou arquivos corrompidos e reparou");

        bool sfcSemProblema =
            texto.Contains("windows resource protection did not find any integrity violations") ||
            texto.Contains("não encontrou violações de integridade") ||
            texto.Contains("nao encontrou violacoes de integridade") ||
            texto.Contains("não encontrou nenhuma violação de integridade") ||
            texto.Contains("nao encontrou nenhuma violacao de integridade");

        bool sfcNaoReparou =
            texto.Contains("windows resource protection found corrupt files but was unable to fix some of them") ||
            texto.Contains("não pôde corrigir alguns") ||
            texto.Contains("nao pode corrigir alguns") ||
            texto.Contains("não conseguiu corrigir alguns") ||
            texto.Contains("nao conseguiu corrigir alguns");

        if (sfcNaoReparou)
            return "⚠️ O SFC encontrou corrupção e não conseguiu corrigir tudo. Reinicie o PC e rode o reparo novamente como Administrador. Se persistir, pode precisar de reparo avançado.";

        if (sfcReparou)
            return "✅ O SFC encontrou arquivos corrompidos e os reparou com êxito. Recomenda-se reiniciar o PC e depois rodar 🩺 Verificar Windows novamente.";

        if (dismOk && sfcSemProblema)
            return "✅ DISM concluiu e o SFC não encontrou violações. O Windows parece corrigido nesta verificação.";

        if (dismOk)
            return "✅ DISM concluiu com sucesso. Leia a saída do SFC acima para confirmar se houve reparo adicional.";

        return "ℹ️ Reparo concluído. Leia a saída acima para confirmar se o Windows pediu reinício ou reparo adicional.";
    }
    private async void OnVerificarWindowsClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Verificação do Windows iniciada";
        ResumoText.Text = "Rodando verificações oficiais do Windows. Isso pode demorar alguns minutos.";
        SistemaText.Text = "Executando DISM e SFC em modo verificação...";
        RelatorioBox.Text = "Verificação do Windows em andamento...";

        if (!OperatingSystem.IsWindows())
        {
            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "VERIFICAÇÃO DO WINDOWS:\n\n" +
                "Esta função só está disponível no Windows.\n" +
                "Nenhuma alteração foi aplicada.\n";

            StatusTitle.Text = "Função indisponível";
            ResumoText.Text = "Esta verificação só funciona no Windows.";
            SistemaText.Text = _ultimoRelatorio;
            RelatorioBox.Text = _ultimoRelatorio;
            return;
        }

        bool admin = EstaRodandoComoAdministrador();

        var sb = new StringBuilder();

        sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
        sb.AppendLine();
        sb.AppendLine("🩺 VERIFICAÇÃO DO WINDOWS — SFC / DISM");
        sb.AppendLine();
        sb.AppendLine(admin
            ? "✅ Permissão: executando como administrador"
            : "⚠️ Permissão: NÃO está executando como administrador");
        sb.AppendLine();

        if (!admin)
        {
            sb.AppendLine("AVISO:");
            sb.AppendLine("Algumas verificações do Windows podem falhar sem permissão de administrador.");
            sb.AppendLine("Se aparecer erro de permissão, feche o app e abra como Administrador.");
            sb.AppendLine();
        }

        sb.AppendLine("MODO:");
        sb.AppendLine("- Esta versão apenas verifica.");
        sb.AppendLine("- Não repara arquivos.");
        sb.AppendLine("- Não altera configurações.");
        sb.AppendLine("- Não reinicia o PC.");
        sb.AppendLine();

        RelatorioBox.Text = sb.ToString();

        StatusTitle.Text = "Rodando DISM CheckHealth";
        ResumoText.Text = "Verificando se a imagem do Windows tem corrupção sinalizada...";
        string dismCheck = await RunCommandAsync("dism", "/Online /Cleanup-Image /CheckHealth");

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("DISM /Online /Cleanup-Image /CheckHealth");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(LimparSaida(dismCheck));
        sb.AppendLine();

        RelatorioBox.Text = sb.ToString();

        StatusTitle.Text = "Rodando SFC VerifyOnly";
        ResumoText.Text = "Verificando integridade dos arquivos do sistema. Pode demorar.";
        string sfcVerify = await RunCommandAsync("sfc", "/verifyonly");

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("SFC /verifyonly");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(LimparSaida(sfcVerify));
        sb.AppendLine();

        string conclusao = GerarConclusaoVerificacaoWindows(dismCheck, sfcVerify, admin);

        sb.AppendLine("CONCLUSÃO:");
        sb.AppendLine(conclusao);
        sb.AppendLine();
        sb.AppendLine("PRÓXIMO PASSO FUTURO:");
        sb.AppendLine("- v2.5.1 poderá adicionar reparo com confirmação:");
        sb.AppendLine("  DISM /RestoreHealth");
        sb.AppendLine("  sfc /scannow");
        sb.AppendLine("- Reparo só deve rodar como administrador e com confirmação.");

        _ultimoRelatorio = sb.ToString();

        StatusTitle.Text = "Verificação do Windows concluída";
        ResumoText.Text = "Relatório SFC/DISM gerado. Use Copiar ou Salvar Relatório.";
        SistemaText.Text = _ultimoRelatorio;
        RelatorioBox.Text = _ultimoRelatorio;
    }

    private static bool EstaRodandoComoAdministrador()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);

            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string GerarConclusaoVerificacaoWindows(string dism, string sfc, bool admin)
    {
        string texto = (dism + "\n" + sfc).ToLowerInvariant();

        if (!admin && (texto.Contains("administrador") || texto.Contains("administrator") || texto.Contains("elevated") || texto.Contains("740")))
        {
            return "⚠️ A verificação parece ter falhado por falta de permissão. Abra o WarPrep como Administrador e rode novamente.";
        }

        bool achouCorrupcao =
            texto.Contains("corrupt") ||
            texto.Contains("corrup") ||
            texto.Contains("integrity violations") ||
            texto.Contains("violações de integridade") ||
            texto.Contains("repairable") ||
            texto.Contains("reparável");

        bool semProblema =
            texto.Contains("no component store corruption detected") ||
            texto.Contains("não foi detectada corrupção") ||
            texto.Contains("did not find any integrity violations") ||
            texto.Contains("não encontrou violações de integridade");

        if (achouCorrupcao)
            return "⚠️ Possível corrupção/violação detectada. Recomendado rodar o módulo de reparo futuramente como Administrador.";

        if (semProblema)
            return "✅ Nenhum problema claro foi detectado pelo DISM/SFC nesta verificação.";

        return "ℹ️ Verificação concluída. Leia a saída acima para confirmar se o Windows pediu reparo ou permissões.";
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
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
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

        sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
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

        sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
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
            "===== EXILLADOS WARPREP v2.5.1 =====\n" +
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

    private async void OnPrepararClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Lista de preparação gerada";
        ResumoText.Text = "Prévia segura criada. Nenhum processo foi fechado e nenhuma alteração foi aplicada no Windows.";

        string powerPlan = await RunCommandAsync("powercfg", "/getactivescheme");
        string gameMode = LerModoJogo();

        string relatorioPreparacao = GerarRelatorioPreparacao(powerPlan, gameMode);

        SistemaText.Text = relatorioPreparacao;

        RelatorioBox.Text =
            "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
            "PREPARAÇÃO PARA GUERRA:\n" +
            relatorioPreparacao + "\n\n" +
            "OBSERVAÇÃO:\n" +
            "Esta versão ainda não fecha programas, não limpa arquivos e não altera configurações.\n" +
            "Ela apenas separa o que pode ser fechado e o que deve ser protegido.\n";

        _ultimoRelatorio = RelatorioBox.Text;
    }





    private string GerarRelatorioPreparacao(string powerPlan, string gameMode)
    {
        var processos = Process.GetProcesses()
            .Select(p => new ProcessoInfo(p.ProcessName, ObterMemoriaMb(p)))
            .Where(p => p.MemoriaMb > 0)
            .GroupBy(p => p.Nome, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProcessoInfo(g.Key, g.Sum(x => x.MemoriaMb)))
            .OrderByDescending(p => p.MemoriaMb)
            .ToList();

        bool bdo = processos.Any(p => EhBlackDesert(p.Nome));
        bool discord = processos.Any(p => EhDiscord(p.Nome));

        bool energiaAlta =
            powerPlan.Contains("Alto desempenho", StringComparison.OrdinalIgnoreCase) ||
            powerPlan.Contains("High performance", StringComparison.OrdinalIgnoreCase) ||
            powerPlan.Contains("8c5e7fda", StringComparison.OrdinalIgnoreCase);

        var podeFechar = processos
            .Where(p => EhCandidatoParaFechar(p.Nome))
            .OrderByDescending(p => p.MemoriaMb)
            .Take(12)
            .ToList();

        int ocultados = processos.Count(p => EhProcessoProtegido(p.Nome));
        var tempInfo = CalcularTemporariosSeguros();

        int pontos = 100;
        var penalidades = new List<string>();

        if (gameMode != "Ativado")
        {
            pontos -= 15;
            penalidades.Add("-15 Modo Jogo não identificado como ativo");
        }

        if (!energiaAlta)
        {
            pontos -= 15;
            penalidades.Add("-15 Plano de energia não está claramente em Alto desempenho");
        }

        if (!bdo)
        {
            pontos -= 10;
            penalidades.Add("-10 Black Desert não detectado aberto");
        }

        if (!discord)
        {
            pontos -= 5;
            penalidades.Add("-5 Discord não detectado aberto");
        }

        if (podeFechar.Count > 0)
        {
            int descontoApps = Math.Min(20, podeFechar.Count * 4);
            pontos -= descontoApps;
            penalidades.Add($"-{descontoApps} apps candidatos para fechar detectados");
        }

        long chromeMb = podeFechar
            .Where(p => p.Nome.Contains("chrome", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.MemoriaMb);

        if (chromeMb >= 2000)
        {
            pontos -= 10;
            penalidades.Add("-10 Chrome consumindo muita memória");
        }
        else if (chromeMb >= 1000)
        {
            pontos -= 5;
            penalidades.Add("-5 Chrome consumindo memória relevante");
        }

        if (tempInfo.BytesPossiveis >= 1024L * 1024L * 1024L)
        {
            pontos -= 5;
            penalidades.Add("-5 Muitos temporários antigos detectados");
        }

        pontos = Math.Max(0, Math.Min(100, pontos));

        string statusProntidao =
            pontos >= 90 ? "🟢 PRONTO PARA GUERRA" :
            pontos >= 70 ? "🟡 ATENÇÃO — dá para jogar, mas tem ajustes recomendados" :
            "🔴 PRECISA AJUSTE antes da guerra";

        var sb = new StringBuilder();

        sb.AppendLine("🛡️ CHECKLIST DE GUERRA — EXILLADOS v2.5.1");
        sb.AppendLine();

        sb.AppendLine("🧭 SCORE DE PRONTIDÃO:");
        sb.AppendLine($"{statusProntidao}");
        sb.AppendLine($"Pontuação: {pontos}/100");
        sb.AppendLine();

        if (penalidades.Count > 0)
        {
            sb.AppendLine("Pontos de atenção:");
            foreach (var p in penalidades)
                sb.AppendLine(p);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("✅ Nenhum ponto crítico detectado no checklist.");
            sb.AppendLine();
        }

        sb.AppendLine(gameMode == "Ativado"
            ? "✅ Modo Jogo do Windows: Ativado"
            : "⚠️ Modo Jogo do Windows: não identificado como ativo");

        sb.AppendLine(energiaAlta
            ? "✅ Plano de energia: Alto desempenho detectado"
            : "⚠️ Plano de energia: não está claramente em Alto desempenho");

        sb.AppendLine(bdo
            ? "✅ Black Desert detectado"
            : "⚠️ Black Desert não detectado aberto");

        sb.AppendLine(discord
            ? "✅ Discord detectado"
            : "⚠️ Discord não detectado aberto");

        sb.AppendLine();
        sb.AppendLine("🟡 PODE FECHAR SE NÃO ESTIVER USANDO:");

        if (podeFechar.Count == 0)
        {
            sb.AppendLine("- Nenhum navegador, launcher, sincronizador ou torrent pesado detectado.");
        }
        else
        {
            foreach (var p in podeFechar)
                sb.AppendLine($"- {p.Nome}: {p.MemoriaMb} MB — {MotivoPodeFechar(p.Nome)}");
        }

        sb.AppendLine();
        sb.AppendLine("🧹 PRÉVIA DE LIMPEZA SEGURA:");
        sb.AppendLine($"- Arquivos analisados: {tempInfo.ArquivosAnalisados}");
        sb.AppendLine($"- Arquivos antigos possíveis para limpeza: {tempInfo.ArquivosPossiveis}");
        sb.AppendLine($"- Tamanho aproximado possível: {FormatarBytes(tempInfo.BytesPossiveis)}");
        sb.AppendLine("- Nada foi apagado pelo checklist.");

        sb.AppendLine();
        sb.AppendLine($"✅ {ocultados} processos protegidos/sensíveis foram ocultados para evitar fechamento acidental.");
        sb.AppendLine("✅ Segurança: nenhuma alteração foi aplicada pelo checklist.");
        sb.AppendLine("✅ Apps só são fechados se o player marcar manualmente e confirmar.");
        sb.AppendLine("✅ Limpeza segura só executa com confirmação dupla.");
        sb.AppendLine("✅ Energia e Modo Jogo têm backup/restauração quando alterados.");

        return sb.ToString();
    }
    private TempPreview CalcularTemporariosSeguros()
    {
        string pastaTemp = System.IO.Path.GetTempPath();

        long bytes = 0;
        int analisados = 0;
        int possiveis = 0;

        try
        {
            var arquivos = System.IO.Directory.EnumerateFiles(pastaTemp, "*", System.IO.SearchOption.AllDirectories)
                .Take(5000);

            foreach (var arquivo in arquivos)
            {
                analisados++;

                try
                {
                    var info = new System.IO.FileInfo(arquivo);

                    if (!info.Exists)
                        continue;

                    bool antigo = info.LastWriteTime < DateTime.Now.AddHours(-24);

                    if (!antigo)
                        continue;

                    bytes += info.Length;
                    possiveis++;
                }
                catch
                {
                    // Arquivo em uso ou sem permissão. Ignora.
                }
            }
        }
        catch
        {
            // Pasta temp pode ter arquivos protegidos. Ignora falhas.
        }

        return new TempPreview(pastaTemp, analisados, possiveis, bytes);
    }

    private static string FormatarBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        double kb = bytes / 1024.0;
        if (kb < 1024)
            return $"{kb:0.0} KB";

        double mb = kb / 1024.0;
        if (mb < 1024)
            return $"{mb:0.0} MB";

        double gb = mb / 1024.0;
        return $"{gb:0.0} GB";
    }

    private sealed record TempPreview(string PastaTemp, int ArquivosAnalisados, int ArquivosPossiveis, long BytesPossiveis);
    private sealed record CleanupResult(string PastaTemp, int ArquivosAnalisados, int ArquivosRemovidos, int Falhas, long BytesLiberados);
    private sealed record CloseAppsResult(int PedidosEnviados, int Ignorados, List<string> Detalhes);
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






    private void OnFecharAppsClick(object? sender, RoutedEventArgs e)
    {
        _fecharSelecionadosConfirmacaoPendente = false;

        var candidatos = ObterCandidatosParaFechamento();

        AppsSelecionaveisPanel.Children.Clear();

        StatusTitle.Text = "Apps seguros listados";
        ResumoText.Text = "Marque apenas os apps que deseja fechar. Nada será fechado até você clicar em Fechar Apps Marcados.";

        var sb = new StringBuilder();

        sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
        sb.AppendLine();
        sb.AppendLine("LISTA DE APPS SEGUROS PARA MARCAR:");
        sb.AppendLine();

        if (candidatos.Count == 0)
        {
            AppsSelecionaveisAvisoText.Text = "Nenhum app seguro para fechamento foi encontrado agora.";
            sb.AppendLine("Nenhum app seguro para fechamento foi encontrado agora.");
        }
        else
        {
            AppsSelecionaveisAvisoText.Text = "Marque abaixo somente o que você quer fechar. BDO, Discord, antivírus, drivers e Windows ficam protegidos.";

            foreach (var app in candidatos)
            {
                var texto = new TextBlock
                {
                    Text = $"{app.Nome}: {app.MemoriaMb} MB — {MotivoPodeFechar(app.Nome)}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E5E7EB"))
                };

                var check = new CheckBox
                {
                    Content = texto,
                    Tag = app.Nome,
                    IsChecked = false
                };

                AppsSelecionaveisPanel.Children.Add(check);
                sb.AppendLine($"[ ] {app.Nome}: {app.MemoriaMb} MB — {MotivoPodeFechar(app.Nome)}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("SEGURANÇA:");
        sb.AppendLine("- Nenhum processo foi fechado.");
        sb.AppendLine("- O player precisa marcar manualmente cada app.");
        sb.AppendLine("- Depois precisa clicar em Fechar Apps Marcados.");
        sb.AppendLine("- Antes de fechar, o programa ainda pede confirmação.");
        sb.AppendLine("- Black Desert, Discord, antivírus, drivers e Windows continuam protegidos.");

        _ultimoRelatorio = sb.ToString();
        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private void OnFecharSelecionadosClick(object? sender, RoutedEventArgs e)
    {
        var selecionados = AppsSelecionaveisPanel.Children
            .OfType<CheckBox>()
            .Where(c => c.IsChecked == true)
            .Select(c => c.Tag as string)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selecionados.Count == 0)
        {
            _fecharSelecionadosConfirmacaoPendente = false;

            StatusTitle.Text = "Nenhum app marcado";
            ResumoText.Text = "Marque pelo menos um app seguro antes de pedir fechamento.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "Nenhum app foi marcado para fechamento.\n\n" +
                "Use primeiro:\n" +
                "1. 📋 Listar Apps Seguros\n" +
                "2. Marque os apps desejados\n" +
                "3. Clique em ✅ Fechar Apps Marcados\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        if (!_fecharSelecionadosConfirmacaoPendente)
        {
            _fecharSelecionadosConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Confira a lista marcada. Clique novamente em Fechar Apps Marcados para confirmar.";

            var sb = new StringBuilder();

            sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
            sb.AppendLine();
            sb.AppendLine("CONFIRMAÇÃO DE FECHAMENTO DOS APPS MARCADOS:");
            sb.AppendLine();

            foreach (var nome in selecionados)
                sb.AppendLine($"- {nome}");

            sb.AppendLine();
            sb.AppendLine("NADA FOI FECHADO AINDA.");
            sb.AppendLine();
            sb.AppendLine("Para confirmar, clique novamente em:");
            sb.AppendLine("✅ Fechar Apps Marcados");
            sb.AppendLine();
            sb.AppendLine("Travas:");
            sb.AppendLine("- Só fecha apps marcados manualmente.");
            sb.AppendLine("- Não fecha Black Desert.");
            sb.AppendLine("- Não fecha Discord.");
            sb.AppendLine("- Não fecha antivírus, drivers, Windows ou processos sensíveis.");
            sb.AppendLine("- Não usa kill forçado nesta versão.");

            _ultimoRelatorio = sb.ToString();
            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        _fecharSelecionadosConfirmacaoPendente = false;

        var resultado = FecharAppsSelecionados(selecionados);

        StatusTitle.Text = "Fechamento dos marcados concluído";
        ResumoText.Text = "O WarPrep tentou fechar apenas os apps marcados manualmente.";

        var rel = new StringBuilder();

        rel.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
        rel.AppendLine();
        rel.AppendLine("RESULTADO DO FECHAMENTO DOS APPS MARCADOS:");
        rel.AppendLine();
        rel.AppendLine($"Apps marcados: {selecionados.Count}");
        rel.AppendLine($"Pedidos de fechamento enviados: {resultado.PedidosEnviados}");
        rel.AppendLine($"Ignorados/sem janela: {resultado.Ignorados}");
        rel.AppendLine();

        if (resultado.Detalhes.Count > 0)
        {
            rel.AppendLine("Detalhes:");
            foreach (var linha in resultado.Detalhes)
                rel.AppendLine("- " + linha);
        }

        rel.AppendLine();
        rel.AppendLine("Segurança:");
        rel.AppendLine("- Não foi usado kill forçado.");
        rel.AppendLine("- Apenas apps marcados manualmente foram tentados.");
        rel.AppendLine("- Black Desert, Discord, antivírus, drivers e Windows ficaram protegidos.");

        _ultimoRelatorio = rel.ToString();
        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private CloseAppsResult FecharAppsSelecionados(List<string> selecionados)
    {
        int pedidos = 0;
        int ignorados = 0;
        var detalhes = new List<string>();

        var nomes = selecionados
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var processo in Process.GetProcesses())
        {
            try
            {
                if (!nomes.Contains(processo.ProcessName))
                    continue;

                if (!EhCandidatoParaFechar(processo.ProcessName))
                    continue;

                if (processo.HasExited)
                    continue;

                if (processo.MainWindowHandle == IntPtr.Zero)
                {
                    ignorados++;
                    detalhes.Add($"{processo.ProcessName}: ignorado porque não tinha janela principal");
                    continue;
                }

                bool enviado = processo.CloseMainWindow();

                if (enviado)
                {
                    pedidos++;
                    detalhes.Add($"{processo.ProcessName}: pedido de fechamento amigável enviado");
                }
                else
                {
                    ignorados++;
                    detalhes.Add($"{processo.ProcessName}: não aceitou fechamento amigável");
                }
            }
            catch
            {
                ignorados++;
            }
        }

        return new CloseAppsResult(pedidos, ignorados, detalhes);
    }
    private List<ProcessoInfo> ObterCandidatosParaFechamento()
    {
        return Process.GetProcesses()
            .Select(p => new ProcessoInfo(p.ProcessName, ObterMemoriaMb(p)))
            .Where(p => p.MemoriaMb > 0)
            .GroupBy(p => p.Nome, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProcessoInfo(g.Key, g.Sum(x => x.MemoriaMb)))
            .Where(p => EhCandidatoParaFechar(p.Nome))
            .OrderByDescending(p => p.MemoriaMb)
            .Take(12)
            .ToList();
    }

    private CloseAppsResult FecharAppsSeguros(List<ProcessoInfo> candidatos)
    {
        int pedidos = 0;
        int ignorados = 0;
        var detalhes = new List<string>();

        var nomes = candidatos
            .Select(c => c.Nome)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var processo in Process.GetProcesses())
        {
            try
            {
                if (!nomes.Contains(processo.ProcessName))
                    continue;

                if (!EhCandidatoParaFechar(processo.ProcessName))
                    continue;

                if (processo.HasExited)
                    continue;

                if (processo.MainWindowHandle == IntPtr.Zero)
                {
                    ignorados++;
                    detalhes.Add($"{processo.ProcessName}: ignorado porque não tinha janela principal");
                    continue;
                }

                bool enviado = processo.CloseMainWindow();

                if (enviado)
                {
                    pedidos++;
                    detalhes.Add($"{processo.ProcessName}: pedido de fechamento enviado");
                }
                else
                {
                    ignorados++;
                    detalhes.Add($"{processo.ProcessName}: não aceitou fechamento amigável");
                }
            }
            catch
            {
                ignorados++;
            }
        }

        return new CloseAppsResult(pedidos, ignorados, detalhes);
    }
    private void OnLimparTempClick(object? sender, RoutedEventArgs e)
    {
        var preview = CalcularTemporariosSeguros();

        if (!_limpezaConfirmacaoPendente)
        {
            _limpezaConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Clique novamente em Limpar Temporários Seguros para confirmar. Apenas arquivos temporários antigos do usuário serão removidos.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "CONFIRMAÇÃO DE LIMPEZA SEGURA:\n\n" +
                $"Pasta temporária analisada: {preview.PastaTemp}\n" +
                $"Arquivos analisados: {preview.ArquivosAnalisados}\n" +
                $"Arquivos antigos possíveis para limpeza: {preview.ArquivosPossiveis}\n" +
                $"Tamanho aproximado possível: {FormatarBytes(preview.BytesPossiveis)}\n\n" +
                "NADA FOI APAGADO AINDA.\n\n" +
                "Para confirmar, clique novamente no botão:\n" +
                "🧹 Limpar Temporários Seguros\n\n" +
                "Segurança:\n" +
                "- Só limpa arquivos da pasta temporária do usuário.\n" +
                "- Só tenta remover arquivos antigos, com mais de 24 horas.\n" +
                "- Arquivos em uso ou bloqueados são ignorados.\n" +
                "- Não mexe no BDO, Discord, antivírus, drivers ou arquivos do sistema.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        _limpezaConfirmacaoPendente = false;

        var resultado = LimparTemporariosSeguros();

        StatusTitle.Text = "Limpeza segura concluída";
        ResumoText.Text = "Arquivos temporários antigos foram limpos. Arquivos bloqueados/em uso foram ignorados.";

        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
            "RESULTADO DA LIMPEZA SEGURA:\n\n" +
            $"Pasta temporária: {resultado.PastaTemp}\n" +
            $"Arquivos encontrados/analisados: {resultado.ArquivosAnalisados}\n" +
            $"Arquivos removidos: {resultado.ArquivosRemovidos}\n" +
            $"Falhas/ignorados: {resultado.Falhas}\n" +
            $"Espaço aproximado liberado: {FormatarBytes(resultado.BytesLiberados)}\n\n" +
            "Segurança:\n" +
            "- Somente temporários antigos do usuário foram tentados.\n" +
            "- Arquivos em uso foram ignorados.\n" +
            "- Nenhum processo foi fechado.\n" +
            "- Nenhuma configuração do Windows foi alterada.\n";

        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private CleanupResult LimparTemporariosSeguros()
    {
        string pastaTemp = System.IO.Path.GetTempPath();

        int analisados = 0;
        int removidos = 0;
        int falhas = 0;
        long bytesLiberados = 0;

        try
        {
            var arquivos = System.IO.Directory.EnumerateFiles(pastaTemp, "*", System.IO.SearchOption.AllDirectories)
                .Take(5000);

            foreach (var arquivo in arquivos)
            {
                analisados++;

                try
                {
                    var info = new System.IO.FileInfo(arquivo);

                    if (!info.Exists)
                        continue;

                    bool antigo = info.LastWriteTime < DateTime.Now.AddHours(-24);

                    if (!antigo)
                        continue;

                    long tamanho = info.Length;
                    System.IO.File.Delete(arquivo);

                    removidos++;
                    bytesLiberados += tamanho;
                }
                catch
                {
                    falhas++;
                }
            }
        }
        catch
        {
            falhas++;
        }

        return new CleanupResult(pastaTemp, analisados, removidos, falhas, bytesLiberados);
    }


    private async void OnModoJogoClick(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            StatusTitle.Text = "Modo Jogo indisponível";
            ResumoText.Text = "Esta função só está disponível no Windows.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "MODO JOGO:\n\n" +
                "Esta função só está disponível no Windows.\n" +
                "Nenhuma alteração foi aplicada.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        string estadoAtual = LerModoJogo();
        string? valorAtual = LerValorModoJogoRegistro();

        if (estadoAtual == "Ativado")
        {
            _modoJogoConfirmacaoPendente = false;

            StatusTitle.Text = "Modo Jogo já está ativado";
            ResumoText.Text = "Nenhuma alteração foi necessária.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "MODO JOGO:\n\n" +
                "✅ O Modo Jogo do Windows já está ativado.\n" +
                "Nenhuma alteração foi aplicada.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        if (!_modoJogoConfirmacaoPendente)
        {
            _modoJogoConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Clique novamente em Ativar Modo Jogo para aplicar. O valor atual será salvo para restauração.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "CONFIRMAÇÃO — ATIVAR MODO JOGO:\n\n" +
                "NADA FOI ALTERADO AINDA.\n\n" +
                $"Estado atual detectado: {estadoAtual}\n" +
                $"Valor atual do registro: {(valorAtual ?? "não existente")}\n\n" +
                "Se confirmar, o WarPrep vai:\n" +
                "- Salvar o valor atual para restauração.\n" +
                "- Ativar AutoGameModeEnabled no usuário atual do Windows.\n\n" +
                "Para confirmar, clique novamente em:\n" +
                "🎮 Ativar Modo Jogo\n\n" +
                "Restauração:\n" +
                "- Depois você pode usar ↩ Restaurar Configurações para voltar ao valor salvo.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        _modoJogoConfirmacaoPendente = false;

        SalvarBackupModoJogo(valorAtual);

        string resultado = AplicarModoJogoAtivo();
        string depois = LerModoJogo();

        StatusTitle.Text = "Modo Jogo aplicado";
        ResumoText.Text = "O WarPrep tentou ativar o Modo Jogo. Use Restaurar Configurações para voltar ao valor anterior salvo.";

        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
            "RESULTADO — ATIVAR MODO JOGO:\n\n" +
            $"Valor anterior salvo: {(valorAtual ?? "não existente")}\n\n" +
            $"Resultado: {resultado}\n" +
            $"Estado após tentativa: {depois}\n\n" +
            "Segurança:\n" +
            "- Nenhum processo foi fechado.\n" +
            "- Nenhum arquivo foi apagado.\n" +
            "- O valor anterior foi salvo para restauração.\n";

        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private async void OnEnergiaClick(object? sender, RoutedEventArgs e)
    {
        const string altoDesempenhoGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";

        string planoAtualTexto = await RunCommandAsync("powercfg", "/getactivescheme");
        string? guidAtual = ExtrairGuidPlanoEnergia(planoAtualTexto);

        if (string.IsNullOrWhiteSpace(guidAtual))
        {
            StatusTitle.Text = "Não foi possível identificar o plano atual";
            ResumoText.Text = "O WarPrep não aplicou nenhuma alteração.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "ENERGIA ALTO DESEMPENHO:\n\n" +
                "Não foi possível identificar o plano de energia atual.\n" +
                "Nenhuma alteração foi aplicada.\n\n" +
                $"Saída do powercfg:\n{planoAtualTexto}\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        if (guidAtual.Equals(altoDesempenhoGuid, StringComparison.OrdinalIgnoreCase) ||
            planoAtualTexto.Contains("Alto desempenho", StringComparison.OrdinalIgnoreCase) ||
            planoAtualTexto.Contains("High performance", StringComparison.OrdinalIgnoreCase))
        {
            _energiaConfirmacaoPendente = false;

            StatusTitle.Text = "Energia já está em Alto Desempenho";
            ResumoText.Text = "Nenhuma alteração foi necessária.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "ENERGIA ALTO DESEMPENHO:\n\n" +
                "✅ O plano de energia atual já parece estar em Alto Desempenho.\n" +
                "Nenhuma alteração foi aplicada.\n\n" +
                $"Plano atual:\n{planoAtualTexto}\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        if (!_energiaConfirmacaoPendente)
        {
            _energiaConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Clique novamente em Energia Alto Desempenho para aplicar. O plano atual será salvo para restauração.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "CONFIRMAÇÃO — ENERGIA ALTO DESEMPENHO:\n\n" +
                "NADA FOI ALTERADO AINDA.\n\n" +
                "Plano atual detectado:\n" +
                $"{planoAtualTexto}\n\n" +
                "Se confirmar, o WarPrep vai:\n" +
                "- Salvar o plano atual para restauração.\n" +
                "- Tentar aplicar o plano Alto Desempenho do Windows.\n\n" +
                "Para confirmar, clique novamente em:\n" +
                "⚡ Energia Alto Desempenho\n\n" +
                "Restauração:\n" +
                "- Depois você pode usar ↩ Restaurar Configurações para voltar ao plano salvo.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        _energiaConfirmacaoPendente = false;

        SalvarBackupPlanoEnergia(guidAtual);

        string resultado = await RunCommandAsync("powercfg", $"/setactive {altoDesempenhoGuid}");
        string depois = await RunCommandAsync("powercfg", "/getactivescheme");

        StatusTitle.Text = "Energia aplicada";
        ResumoText.Text = "O WarPrep tentou aplicar Alto Desempenho. Use Restaurar Configurações para voltar ao plano anterior salvo.";

        _ultimoRelatorio =
            "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
            "RESULTADO — ENERGIA ALTO DESEMPENHO:\n\n" +
            $"Plano anterior salvo: {guidAtual}\n\n" +
            "Resultado do comando:\n" +
            $"{resultado}\n\n" +
            "Plano atual após tentativa:\n" +
            $"{depois}\n\n" +
            "Segurança:\n" +
            "- Nenhum processo foi fechado.\n" +
            "- Nenhum arquivo foi apagado.\n" +
            "- O plano anterior foi salvo para restauração.\n";

        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private async void OnRestaurarClick(object? sender, RoutedEventArgs e)
    {
        string? guidBackup = LerBackupPlanoEnergia();
        string? modoJogoBackup = LerBackupModoJogo();

        bool temEnergia = !string.IsNullOrWhiteSpace(guidBackup);
        bool temModoJogo = modoJogoBackup is not null;

        if (!temEnergia && !temModoJogo)
        {
            _restaurarEnergiaConfirmacaoPendente = false;

            StatusTitle.Text = "Nada para restaurar";
            ResumoText.Text = "Nenhuma configuração anterior foi encontrada no backup local.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "RESTAURAR CONFIGURAÇÕES:\n\n" +
                "Nenhum backup local de energia ou Modo Jogo foi encontrado.\n" +
                "Nenhuma alteração foi aplicada.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        if (!_restaurarEnergiaConfirmacaoPendente)
        {
            _restaurarEnergiaConfirmacaoPendente = true;

            StatusTitle.Text = "Confirmação necessária";
            ResumoText.Text = "Clique novamente em Restaurar Configurações para voltar ao que foi salvo.";

            var sb = new StringBuilder();

            sb.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
            sb.AppendLine();
            sb.AppendLine("CONFIRMAÇÃO — RESTAURAR CONFIGURAÇÕES:");
            sb.AppendLine();
            sb.AppendLine("NADA FOI ALTERADO AINDA.");
            sb.AppendLine();

            if (temEnergia)
                sb.AppendLine($"- Plano de energia salvo: {guidBackup}");

            if (temModoJogo)
                sb.AppendLine($"- Modo Jogo salvo: {modoJogoBackup}");

            sb.AppendLine();
            sb.AppendLine("Para confirmar, clique novamente em:");
            sb.AppendLine("↩ Restaurar Configurações");

            _ultimoRelatorio = sb.ToString();
            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
            return;
        }

        _restaurarEnergiaConfirmacaoPendente = false;

        var resultado = new StringBuilder();

        resultado.AppendLine("===== EXILLADOS WARPREP v2.5.1 =====");
        resultado.AppendLine();
        resultado.AppendLine("RESULTADO — RESTAURAR CONFIGURAÇÕES:");
        resultado.AppendLine();

        if (temEnergia)
        {
            string resEnergia = await RunCommandAsync("powercfg", $"/setactive {guidBackup}");
            string depoisEnergia = await RunCommandAsync("powercfg", "/getactivescheme");

            resultado.AppendLine("Energia:");
            resultado.AppendLine($"- Plano restaurado: {guidBackup}");
            resultado.AppendLine($"- Resultado: {resEnergia}");
            resultado.AppendLine($"- Plano atual: {depoisEnergia}");
            resultado.AppendLine();

            LimparBackupPlanoEnergia();
        }

        if (temModoJogo)
        {
            string resModo = RestaurarModoJogo(modoJogoBackup);

            resultado.AppendLine("Modo Jogo:");
            resultado.AppendLine($"- Valor restaurado: {modoJogoBackup}");
            resultado.AppendLine($"- Resultado: {resModo}");
            resultado.AppendLine($"- Estado atual: {LerModoJogo()}");
            resultado.AppendLine();

            LimparBackupModoJogo();
        }

        resultado.AppendLine("Segurança:");
        resultado.AppendLine("- Nenhum processo foi fechado.");
        resultado.AppendLine("- Nenhum arquivo foi apagado.");
        resultado.AppendLine("- Backups locais usados foram removidos após restauração.");

        StatusTitle.Text = "Configurações restauradas";
        ResumoText.Text = "O WarPrep tentou restaurar as configurações salvas.";

        _ultimoRelatorio = resultado.ToString();
        RelatorioBox.Text = _ultimoRelatorio;
        SistemaText.Text = _ultimoRelatorio;
    }

    private static string? ExtrairGuidPlanoEnergia(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}"
        );

        return match.Success ? match.Value : null;
    }

    private static string ObterPastaEstado()
    {
        string pasta = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ExilladosWarPrep"
        );

        System.IO.Directory.CreateDirectory(pasta);
        return pasta;
    }

    private static string ObterArquivoBackupEnergia()
    {
        return System.IO.Path.Combine(ObterPastaEstado(), "energia_backup.txt");
    }

    private static void SalvarBackupPlanoEnergia(string guid)
    {
        System.IO.File.WriteAllText(ObterArquivoBackupEnergia(), guid);
    }

    private static string? LerBackupPlanoEnergia()
    {
        string arquivo = ObterArquivoBackupEnergia();

        if (!System.IO.File.Exists(arquivo))
            return null;

        string texto = System.IO.File.ReadAllText(arquivo).Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static void LimparBackupPlanoEnergia()
    {
        string arquivo = ObterArquivoBackupEnergia();

        if (System.IO.File.Exists(arquivo))
            System.IO.File.Delete(arquivo);
    }

    private static string ObterArquivoBackupModoJogo()
    {
        return System.IO.Path.Combine(ObterPastaEstado(), "modo_jogo_backup.txt");
    }

    private static string? LerValorModoJogoRegistro()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        return LerValorModoJogoRegistroWindows();
    }

    [SupportedOSPlatform("windows")]
    private static string? LerValorModoJogoRegistroWindows()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\GameBar");
            var valor = key?.GetValue("AutoGameModeEnabled");

            return valor?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static void SalvarBackupModoJogo(string? valorAtual)
    {
        System.IO.File.WriteAllText(ObterArquivoBackupModoJogo(), valorAtual ?? "missing");
    }

    private static string? LerBackupModoJogo()
    {
        string arquivo = ObterArquivoBackupModoJogo();

        if (!System.IO.File.Exists(arquivo))
            return null;

        return System.IO.File.ReadAllText(arquivo).Trim();
    }

    private static void LimparBackupModoJogo()
    {
        string arquivo = ObterArquivoBackupModoJogo();

        if (System.IO.File.Exists(arquivo))
            System.IO.File.Delete(arquivo);
    }

    private static string AplicarModoJogoAtivo()
    {
        if (!OperatingSystem.IsWindows())
            return "Indisponível fora do Windows.";

        return SetModoJogoValorWindows("1");
    }

    private static string RestaurarModoJogo(string valorBackup)
    {
        if (!OperatingSystem.IsWindows())
            return "Indisponível fora do Windows.";

        if (valorBackup == "missing")
            return RemoverModoJogoValorWindows();

        return SetModoJogoValorWindows(valorBackup);
    }

    [SupportedOSPlatform("windows")]
    private static string SetModoJogoValorWindows(string valor)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\GameBar");
            key?.SetValue("AutoGameModeEnabled", int.Parse(valor), RegistryValueKind.DWord);

            return "Valor aplicado com sucesso.";
        }
        catch (Exception ex)
        {
            return $"Erro ao aplicar valor: {ex.Message}";
        }
    }

    [SupportedOSPlatform("windows")]
    private static string RemoverModoJogoValorWindows()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\GameBar", writable: true);
            key?.DeleteValue("AutoGameModeEnabled", throwOnMissingValue: false);

            return "Valor removido para voltar ao estado anterior.";
        }
        catch (Exception ex)
        {
            return $"Erro ao remover valor: {ex.Message}";
        }
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


    private void OnSalvarRelatorioClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            string pasta = ObterPastaRelatorios();

            System.IO.Directory.CreateDirectory(pasta);

            string nomeArquivo = $"warprep-relatorio-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            string caminho = System.IO.Path.Combine(pasta, nomeArquivo);

            string conteudo = string.IsNullOrWhiteSpace(_ultimoRelatorio)
                ? "Nenhum relatório gerado ainda."
                : _ultimoRelatorio;

            System.IO.File.WriteAllText(caminho, conteudo, Encoding.UTF8);

            StatusTitle.Text = "Relatório salvo";
            ResumoText.Text = "O relatório foi salvo na Área de Trabalho, na pasta Exillados WarPrep - Relatorios.";

            _ultimoRelatorio =
                conteudo +
                "\n\n===== SALVO EM ARQUIVO =====\n" +
                $"Caminho: {caminho}\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = "Relatório salvo com sucesso:\n\n" + caminho;
        }
        catch (Exception ex)
        {
            StatusTitle.Text = "Erro ao salvar relatório";
            ResumoText.Text = "Não foi possível salvar o arquivo.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "ERRO AO SALVAR RELATÓRIO:\n\n" +
                ex.Message + "\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
        }
    }

    private void OnAbrirRelatoriosClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            string pasta = ObterPastaRelatorios();

            System.IO.Directory.CreateDirectory(pasta);

            Process.Start(new ProcessStartInfo
            {
                FileName = pasta,
                UseShellExecute = true
            });

            StatusTitle.Text = "Pasta de relatórios aberta";
            ResumoText.Text = "A pasta de relatórios do Exillados WarPrep foi aberta na Área de Trabalho.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "PASTA DE RELATÓRIOS:\n\n" +
                $"Caminho: {pasta}\n\n" +
                "Use esta pasta para encontrar os relatórios .txt salvos pelo WarPrep.\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
        }
        catch (Exception ex)
        {
            StatusTitle.Text = "Erro ao abrir relatórios";
            ResumoText.Text = "Não foi possível abrir a pasta de relatórios.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v2.5.1 =====\n\n" +
                "ERRO AO ABRIR PASTA DE RELATÓRIOS:\n\n" +
                ex.Message + "\n";

            RelatorioBox.Text = _ultimoRelatorio;
            SistemaText.Text = _ultimoRelatorio;
        }
    }

    private static string ObterPastaRelatorios()
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        return System.IO.Path.Combine(desktop, "Exillados WarPrep - Relatorios");
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
    private sealed record ModoTeste(string Nome, int Pacotes, int TracertSaltos);
    private sealed record ProcessoInfo(string Nome, long MemoriaMb);

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



























