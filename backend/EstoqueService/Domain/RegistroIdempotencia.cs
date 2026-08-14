namespace EstoqueService.Domain;

/// <summary>
/// Guarda o resultado de uma operação de débito já processada, indexada pela
/// chave de idempotência. Garante que reenvios (ex.: clique duplo em "Imprimir")
/// não deem baixa dupla no estoque.
/// </summary>
public class RegistroIdempotencia
{
    public int Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string ResultadoJson { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
