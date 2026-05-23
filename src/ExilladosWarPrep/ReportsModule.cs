using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.Text;

namespace ExilladosWarPrep;

public partial class MainWindow
{
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
                "===== EXILLADOS WARPREP v2.8 =====\n\n" +
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
                "===== EXILLADOS WARPREP v2.8 =====\n\n" +
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
                "===== EXILLADOS WARPREP v2.8 =====\n\n" +
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
}





