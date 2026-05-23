using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExilladosWarPrep;

public partial class MainWindow
{
    private void AtualizarPainelProcessosVisuais(List<ProcessoInfo> processos)
    {
        try
        {
            ProcessosVisuaisPanel.Children.Clear();

            var protegidos = processos
                .Where(p => EhProcessoProtegido(p.Nome) || EhBlackDesert(p.Nome) || EhDiscord(p.Nome))
                .OrderByDescending(p => p.MemoriaMb)
                .Take(8)
                .ToList();

            var recomendados = processos
                .Where(p => !EhProcessoProtegido(p.Nome))
                .Where(p => !EhBlackDesert(p.Nome))
                .Where(p => !EhDiscord(p.Nome))
                .Where(p => EhCandidatoParaFechar(p.Nome))
                .Where(EhRecomendadoFecharVisual)
                .OrderByDescending(p => p.MemoriaMb)
                .Take(8)
                .ToList();

            var recomendadosNomes = recomendados
                .Select(p => p.Nome)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var verificar = processos
                .Where(p => !EhProcessoProtegido(p.Nome))
                .Where(p => !EhBlackDesert(p.Nome))
                .Where(p => !EhDiscord(p.Nome))
                .Where(p => !recomendadosNomes.Contains(p.Nome))
                .Where(p => EhCandidatoParaFechar(p.Nome) || p.MemoriaMb >= 300)
                .OrderByDescending(p => p.MemoriaMb)
                .Take(8)
                .ToList();

            if (protegidos.Count == 0 && verificar.Count == 0 && recomendados.Count == 0)
            {
                ProcessosVisuaisAvisoText.Text = "Nenhum processo relevante foi classificado agora.";
                return;
            }

            ProcessosVisuaisAvisoText.Text = "Legenda: verde = não fechar, amarelo = verificar se precisa, vermelho = recomendado fechar antes da guerra.";

            AdicionarGrupoProcessosVisual(
                "🟢 NÃO FECHAR",
                "Protegidos, jogo, comunicação da guild, Windows, drivers ou segurança.",
                protegidos,
                "#052E16",
                "#86EFAC",
                MotivoVisualProtegido
            );

            AdicionarGrupoProcessosVisual(
                "🟡 VERIFICAR SE PRECISA",
                "Pode consumir RAM/CPU, mas depende se você está usando.",
                verificar,
                "#3B2F08",
                "#FDE68A",
                MotivoVisualVerificar
            );

            AdicionarGrupoProcessosVisual(
                "🔴 RECOMENDADO FECHAR",
                "Candidatos fortes para fechar antes da Node se não forem necessários.",
                recomendados,
                "#3F1111",
                "#FCA5A5",
                MotivoVisualRecomendado
            );
        }
        catch
        {
            // Não deixa painel visual quebrar o checklist.
        }
    }

    private void AdicionarGrupoProcessosVisual(
        string titulo,
        string subtitulo,
        List<ProcessoInfo> itens,
        string corFundo,
        string corTitulo,
        Func<ProcessoInfo, string> motivo)
    {
        if (itens.Count == 0)
            return;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse(corFundo)),
            CornerRadius = new Avalonia.CornerRadius(12),
            Padding = new Avalonia.Thickness(10)
        };

        var stack = new StackPanel
        {
            Spacing = 5
        };

        stack.Children.Add(new TextBlock
        {
            Text = titulo,
            Foreground = new SolidColorBrush(Color.Parse(corTitulo)),
            FontWeight = FontWeight.Bold,
            FontSize = 14
        });

        stack.Children.Add(new TextBlock
        {
            Text = subtitulo,
            Foreground = new SolidColorBrush(Color.Parse("#CBD5E1")),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12
        });

        foreach (var p in itens)
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"• {p.Nome}: {p.MemoriaMb} MB — {motivo(p)}",
                Foreground = new SolidColorBrush(Color.Parse("#F8FAFC")),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            });
        }

        border.Child = stack;
        ProcessosVisuaisPanel.Children.Add(border);
    }

    private static bool EhRecomendadoFecharVisual(ProcessoInfo p)
    {
        string n = p.Nome.ToLowerInvariant();

        if (p.MemoriaMb >= 1000)
            return true;

        return n.Contains("torrent") ||
               n.Contains("utorrent") ||
               n.Contains("qbittorrent") ||
               n.Contains("launcher") ||
               n.Contains("updater") ||
               n.Contains("update") ||
               n.Contains("onedrive") ||
               n.Contains("dropbox") ||
               n.Contains("googledrive") ||
               n.Contains("steam") ||
               n.Contains("epic");
    }

    private static string MotivoVisualProtegido(ProcessoInfo p)
    {
        string n = p.Nome.ToLowerInvariant();

        if (n.Contains("blackdesert"))
            return "jogo detectado, nunca fechar pelo WarPrep";

        if (n.Contains("discord"))
            return "comunicação da guild, não fechar automaticamente";

        if (n.Contains("explorer") || n.Contains("svchost") || n.Contains("dwm") || n.Contains("runtimebroker"))
            return "processo do Windows";

        if (n.Contains("avp") || n.Contains("kaspersky") || n.Contains("msmpeng") || n.Contains("security"))
            return "antivírus/segurança";

        if (n.Contains("nvidia") || n.Contains("nvcontainer") || n.Contains("amd") || n.Contains("radeon"))
            return "driver/placa de vídeo";

        if (n.Contains("devenv") || n.Contains("dotnet") || n.Contains("vbcscompiler") || n.Contains("exilladoswarprep"))
            return "ferramenta do próprio app/desenvolvimento";

        return "processo protegido/sensível";
    }

    private static string MotivoVisualVerificar(ProcessoInfo p)
    {
        if (EhCandidatoParaFechar(p.Nome))
            return MotivoPodeFechar(p.Nome);

        return "processo pesado, verifique se precisa ficar aberto";
    }

    private static string MotivoVisualRecomendado(ProcessoInfo p)
    {
        return MotivoPodeFechar(p.Nome);
    }
}

