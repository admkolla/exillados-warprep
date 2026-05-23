using Avalonia.Interactivity;
using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ExilladosWarPrep;

public partial class MainWindow
{
    private async void OnModoJogoClick(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            StatusTitle.Text = "Modo Jogo indisponível";
            ResumoText.Text = "Esta função só está disponível no Windows.";

            _ultimoRelatorio =
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
            "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
            "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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
                "===== EXILLADOS WARPREP v3.0 =====\n\n" +
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

            sb.AppendLine("===== EXILLADOS WARPREP v3.0 =====");
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

        resultado.AppendLine("===== EXILLADOS WARPREP v3.0 =====");
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
            string resModo = RestaurarModoJogo(modoJogoBackup!);

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
}







