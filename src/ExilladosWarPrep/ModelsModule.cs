using System.Collections.Generic;

namespace ExilladosWarPrep;

public partial class MainWindow
{
    private sealed record ModoTeste(string Nome, int Pacotes, int TracertSaltos);

    private sealed record ProcessoInfo(string Nome, long MemoriaMb);

    private sealed record TempPreview(string PastaTemp, int ArquivosAnalisados, int ArquivosPossiveis, long BytesPossiveis);

    private sealed record CleanupResult(string PastaTemp, int ArquivosAnalisados, int ArquivosRemovidos, int Falhas, long BytesLiberados);

    private sealed record CloseAppsResult(int PedidosEnviados, int Ignorados, List<string> Detalhes);
}




