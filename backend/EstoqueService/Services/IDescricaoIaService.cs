using EstoqueService.DTOs;

namespace EstoqueService.Services;

public interface IDescricaoIaService
{
    Task<GerarDescricaoResponseDto> GerarAsync(GerarDescricaoRequestDto request);
}
