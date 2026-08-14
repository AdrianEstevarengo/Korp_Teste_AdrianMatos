namespace EstoqueService.Domain;

/// <summary>
/// Produto controlado pelo microsserviço de Estoque.
/// O saldo é decrementado quando uma nota fiscal é impressa (fechada).
/// </summary>
public class Produto
{
    public int Id { get; set; }

    /// <summary>Código único do produto (ex.: "P001").</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Descrição / nome do produto.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Quantidade disponível em estoque. Nunca pode ficar negativa.</summary>
    public int Saldo { get; set; }
}
