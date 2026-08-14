using FaturamentoService.DTOs;

namespace FaturamentoService.Services;

public interface INotaFiscalService
{
    Task<IEnumerable<NotaFiscalResponseDto>> ListarAsync();
    Task<NotaFiscalResponseDto> ObterAsync(int id);
    Task<NotaFiscalResponseDto> CriarAsync(CriarNotaDto dto);

    /// <summary>
    /// Imprime a nota: valida que está Aberta, dá baixa no estoque (via serviço
    /// de Estoque, com idempotência) e fecha a nota. Em caso de indisponibilidade
    /// do Estoque, a nota permanece Aberta.
    /// </summary>
    Task<NotaFiscalResponseDto> ImprimirAsync(int id);
}
