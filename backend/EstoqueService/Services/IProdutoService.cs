using EstoqueService.DTOs;

namespace EstoqueService.Services;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoResponseDto>> ListarAsync();
    Task<ProdutoResponseDto> ObterAsync(int id);
    Task<ProdutoResponseDto> CriarAsync(ProdutoCreateDto dto);
    Task<ProdutoResponseDto> AtualizarAsync(int id, ProdutoUpdateDto dto);

    /// <summary>
    /// Dá baixa no saldo de vários produtos de forma atômica.
    /// Trata concorrência (otimista, via xmin) e idempotência (via chave).
    /// </summary>
    Task<DebitarSaldoResponseDto> DebitarAsync(DebitarSaldoRequestDto request, string? chaveIdempotencia);
}
