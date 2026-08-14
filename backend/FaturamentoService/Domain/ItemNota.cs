namespace FaturamentoService.Domain;

public class ItemNota
{
    public int Id { get; set; }

    public int NotaFiscalId { get; set; }
    public NotaFiscal? NotaFiscal { get; set; }

    /// <summary>Id do produto no serviço de Estoque.</summary>
    public int ProdutoId { get; set; }

    /// <summary>Snapshot do código e descrição no momento da emissão (para impressão).</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public int Quantidade { get; set; }
}
