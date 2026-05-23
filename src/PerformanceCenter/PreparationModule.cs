using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceCenter;

public partial class MainWindow
{
    private async void OnPrepararClick(object? sender, RoutedEventArgs e)
    {
        StatusTitle.Text = "Lista de preparação gerada";
        ResumoText.Text = "Prévia segura criada. Nenhum processo foi fechado e nenhuma alteração foi aplicada no Windows.";

        string powerPlan = await RunCommandAsync("powercfg", "/getactivescheme");
        string gameMode = LerModoJogo();

        string relatorioPreparacao = GerarRelatorioPreparacao(powerPlan, gameMode);

        SistemaText.Text = relatorioPreparacao;

        RelatorioBox.Text =
            "===== Performance Center v3.4.4 =====\n\n" +
            "PREPARAÇÃO PARA uso intensivo:\n" +
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
        AtualizarPainelProcessosVisuais(processos);
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
            pontos >= 90 ? "🟢 PRONTO PARA uso intensivo" :
            pontos >= 70 ? "🟡 ATENÇÃO — dá para jogar, mas tem ajustes recomendados" :
            "🔴 PRECISA AJUSTE antes da uso intensivo";

        var sb = new StringBuilder();

        sb.AppendLine("🛡️ CHECKLIST DE uso intensivo — Performance Center v3.4.4");
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

    private void OnFecharAppsClick(object? sender, RoutedEventArgs e)
    {
        _fecharSelecionadosConfirmacaoPendente = false;

        var candidatos = ObterCandidatosParaFechamento();

        AppsSelecionaveisPanel.Children.Clear();

        StatusTitle.Text = "Processos analisados listados";
        ResumoText.Text = "Marque apenas os apps que deseja fechar. Nada será fechado até você clicar em Fechar Apps Marcados.";

        var sb = new StringBuilder();

        sb.AppendLine("===== Performance Center v3.4.4 =====");
        sb.AppendLine();
        sb.AppendLine("LISTA DE Processos analisados PARA MARCAR:");
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
            .Select(n => n!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selecionados.Count == 0)
        {
            _fecharSelecionadosConfirmacaoPendente = false;

            StatusTitle.Text = "Nenhum app marcado";
            ResumoText.Text = "Marque pelo menos um app seguro antes de pedir fechamento.";

            _ultimoRelatorio =
                "===== Performance Center v3.4.4 =====\n\n" +
                "Nenhum app foi marcado para fechamento.\n\n" +
                "Use primeiro:\n" +
                "1. 📋 Listar Processos analisados\n" +
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

            sb.AppendLine("===== Performance Center v3.4.4 =====");
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
        ResumoText.Text = "O Performance Center tentou fechar apenas os apps marcados manualmente.";

        var rel = new StringBuilder();

        rel.AppendLine("===== Performance Center v3.4.4 =====");
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
            ResumoText.Text = "Clique novamente em Limpar Temporários para confirmar. Apenas arquivos temporários antigos do usuário serão removidos.";

            _ultimoRelatorio =
                "===== Performance Center v3.4.4 =====\n\n" +
                "CONFIRMAÇÃO DE LIMPEZA SEGURA:\n\n" +
                $"Pasta temporária analisada: {preview.PastaTemp}\n" +
                $"Arquivos analisados: {preview.ArquivosAnalisados}\n" +
                $"Arquivos antigos possíveis para limpeza: {preview.ArquivosPossiveis}\n" +
                $"Tamanho aproximado possível: {FormatarBytes(preview.BytesPossiveis)}\n\n" +
                "NADA FOI APAGADO AINDA.\n\n" +
                "Para confirmar, clique novamente no botão:\n" +
                "🧹 Limpar Temporários\n\n" +
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
            "===== Performance Center v3.4.4 =====\n\n" +
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
}










