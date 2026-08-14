namespace FaturamentoService.Domain;

public class NotaFiscal
{
    public int Id { get; set; }

    /// <summary>Numeração sequencial da nota.</summary>
    public int Numero { get; set; }

    public StatusNota Status { get; set; } = StatusNota.Aberta;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataImpressao { get; set; }

    /// <summary>
    /// Chave de idempotência gerada na primeira tentativa de impressão e
    /// reutilizada em reenvios, evitando baixa dupla no estoque.
    /// </summary>
    public string? ChaveIdempotencia { get; set; }

    public List<ItemNota> Itens { get; set; } = new();
}
