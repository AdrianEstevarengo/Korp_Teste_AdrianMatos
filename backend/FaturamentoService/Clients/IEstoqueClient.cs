namespace FaturamentoService.Clients;

public interface IEstoqueClient
{
    /// <summary>
    /// Dá baixa no saldo dos produtos no serviço de Estoque.
    /// A chave de idempotência é enviada no header "Idempotency-Key".
    /// Lança EstoqueIndisponivelException se o serviço estiver fora do ar,
    /// ou RegraNegocioException/NaoEncontradoException para erros de negócio.
    /// </summary>
    Task<DebitarSaldoResponseDto> DebitarAsync(DebitarSaldoRequestDto request, string chaveIdempotencia);
}
