using EstoqueService.DTOs;
using EstoqueService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtos;
    private readonly IDescricaoIaService _ia;

    public ProdutosController(IProdutoService produtos, IDescricaoIaService ia)
    {
        _produtos = produtos;
        _ia = ia;
    }

    /// <summary>Lista todos os produtos.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoResponseDto>>> Listar()
        => Ok(await _produtos.ListarAsync());

    /// <summary>Obtém um produto pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDto>> Obter(int id)
        => Ok(await _produtos.ObterAsync(id));

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoResponseDto>> Criar([FromBody] ProdutoCreateDto dto)
    {
        var criado = await _produtos.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza descrição e saldo de um produto.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDto>> Atualizar(int id, [FromBody] ProdutoUpdateDto dto)
        => Ok(await _produtos.AtualizarAsync(id, dto));

    /// <summary>
    /// Dá baixa no saldo (chamado pelo serviço de Faturamento na impressão da nota).
    /// A chave de idempotência é lida do header "Idempotency-Key".
    /// </summary>
    [HttpPost("debitar")]
    public async Task<ActionResult<DebitarSaldoResponseDto>> Debitar([FromBody] DebitarSaldoRequestDto request)
    {
        var chave = Request.Headers["Idempotency-Key"].FirstOrDefault();
        return Ok(await _produtos.DebitarAsync(request, chave));
    }

    /// <summary>Gera automaticamente uma descrição de produto usando IA.</summary>
    [HttpPost("gerar-descricao")]
    public async Task<ActionResult<GerarDescricaoResponseDto>> GerarDescricao([FromBody] GerarDescricaoRequestDto dto)
        => Ok(await _ia.GerarAsync(dto));
}
