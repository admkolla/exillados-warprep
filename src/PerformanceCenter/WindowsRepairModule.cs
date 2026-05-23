using Avalonia.Interactivity;
using System;
using System.Security.Principal;
using System.Text;

namespace PerformanceCenter;

public partial class MainWindow
{
    private async void OnRepararWindowsClick(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            StatusTitle.Text = "Reparo indisponível";
            ResumoText.Text = "Esta função só está disponível no Windows.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v3.2 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.2 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.2 =====\n\n" +
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

        sb.AppendLine("===== EXILLADOS WARPREP v3.2 =====");
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
                "===== EXILLADOS WARPREP v3.2 =====\n\n" +
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

        sb.AppendLine("===== EXILLADOS WARPREP v3.2 =====");
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
        sb.AppendLine("- v3.2 poderá adicionar reparo com confirmação:");
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
}







