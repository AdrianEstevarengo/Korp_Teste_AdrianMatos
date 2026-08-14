using FaturamentoService.DTOs;
using FaturamentoService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/notas")]
public class NotasFiscaisController : ControllerBase
{
    private readonly INotaFiscalService _notas;

    public NotasFiscaisController(INotaFiscalService notas) => _notas = notas;

    /// <summary>Lista todas as notas fiscais.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaFiscalResponseDto>>> Listar()
        => Ok(await _notas.ListarAsync());

    /// <summary>Obtém uma nota fiscal pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotaFiscalResponseDto>> Obter(int id)
        => Ok(await _notas.ObterAsync(id));

    /// <summary>Cria uma nota fiscal (numeração sequencial, status inicial Aberta).</summary>
    [HttpPost]
    public async Task<ActionResult<NotaFiscalResponseDto>> Criar([FromBody] CriarNotaDto dto)
    {
        var criada = await _notas.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>
    /// Imprime a nota: dá baixa no estoque e fecha a nota.
    /// Retorna 409 se a nota não estiver Aberta ou faltar saldo,
    /// e 503 se o serviço de Estoque estiver indisponível.
    /// </summary>
    [HttpPost("{id:int}/imprimir")]
    public async Task<ActionResult<NotaFiscalResponseDto>> Imprimir(int id)
        => Ok(await _notas.ImprimirAsync(id));
}
