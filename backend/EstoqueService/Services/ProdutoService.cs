using System.Text.Json;
using EstoqueService.Data;
using EstoqueService.Domain;
using EstoqueService.DTOs;
using EstoqueService.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Services;

public class ProdutoService : IProdutoService
{
    private const int MaxTentativasConcorrencia = 5;

    private readonly EstoqueDbContext _db;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(EstoqueDbContext db, ILogger<ProdutoService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoResponseDto>> ListarAsync()
    {
        // LINQ: projeção direto no banco (IQueryable -> SQL), sem trazer entidades.
        return await _db.Produtos
            .OrderBy(p => p.Codigo)
            .Select(p => new ProdutoResponseDto(p.Id, p.Codigo, p.Descricao, p.Saldo))
            .ToListAsync();
    }

    public async Task<ProdutoResponseDto> ObterAsync(int id)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NaoEncontradoException($"Produto {id} não encontrado.");
        return Map(produto);
    }

    public async Task<ProdutoResponseDto> CriarAsync(ProdutoCreateDto dto)
    {
        var codigo = dto.Codigo.Trim();

        // LINQ: verificação de existência traduzida para EXISTS no SQL.
        if (await _db.Produtos.AnyAsync(p => p.Codigo == codigo))
            throw new RegraNegocioException($"Já existe um produto com o código '{codigo}'.");

        var produto = new Produto
        {
            Codigo = codigo,
            Descricao = dto.Descricao.Trim(),
            Saldo = dto.Saldo
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();
        return Map(produto);
    }

    public async Task<ProdutoResponseDto> AtualizarAsync(int id, ProdutoUpdateDto dto)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NaoEncontradoException($"Produto {id} não encontrado.");

        produto.Descricao = dto.Descricao.Trim();
        produto.Saldo = dto.Saldo;

        await _db.SaveChangesAsync();
        return Map(produto);
    }

    public async Task<DebitarSaldoResponseDto> DebitarAsync(
        DebitarSaldoRequestDto request, string? chaveIdempotencia)
    {
        // 1) IDEMPOTÊNCIA: se a chave já foi processada, devolve o mesmo resultado
        //    sem tocar no estoque de novo.
        if (!string.IsNullOrWhiteSpace(chaveIdempotencia))
        {
            var existente = await _db.RegistrosIdempotencia
                .FirstOrDefaultAsync(r => r.Chave == chaveIdempotencia);

            if (existente is not null)
            {
                _logger.LogInformation(
                    "Débito idempotente: chave {Chave} já processada, retornando resultado anterior.",
                    chaveIdempotencia);
                return JsonSerializer.Deserialize<DebitarSaldoResponseDto>(existente.ResultadoJson)!;
            }
        }

        // 2) CONCORRÊNCIA: tenta o débito; se outra transação alterar a mesma linha,
        //    o EF lança DbUpdateConcurrencyException e nós recarregamos e repetimos.
        for (var tentativa = 1; tentativa <= MaxTentativasConcorrencia; tentativa++)
        {
            try
            {
                return await ExecutarDebitoAsync(request, chaveIdempotencia);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning(
                    "Conflito de concorrência ao debitar (tentativa {Tentativa}). Recarregando...",
                    tentativa);

                // Limpa o rastreamento para recarregar os saldos atuais na próxima volta.
                _db.ChangeTracker.Clear();

                if (tentativa == MaxTentativasConcorrencia)
                    throw new RegraNegocioException(
                        "Não foi possível concluir a baixa de estoque devido a alta concorrência. Tente novamente.");
            }
        }

        // Inatingível, mas o compilador exige.
        throw new RegraNegocioException("Falha inesperada ao debitar estoque.");
    }

    private async Task<DebitarSaldoResponseDto> ExecutarDebitoAsync(
        DebitarSaldoRequestDto request, string? chaveIdempotencia)
    {
        var ids = request.Itens.Select(i => i.ProdutoId).Distinct().ToList();

        // LINQ: carrega todos os produtos envolvidos em uma única consulta (WHERE ... IN).
        var produtos = await _db.Produtos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var resultado = new List<ItemSaldoDto>();

        foreach (var item in request.Itens)
        {
            if (!produtos.TryGetValue(item.ProdutoId, out var produto))
                throw new NaoEncontradoException($"Produto {item.ProdutoId} não encontrado.");

            if (produto.Saldo < item.Quantidade)
                throw new RegraNegocioException(
                    $"Saldo insuficiente para o produto '{produto.Codigo}': " +
                    $"disponível {produto.Saldo}, solicitado {item.Quantidade}.");

            produto.Saldo -= item.Quantidade;
            resultado.Add(new ItemSaldoDto(produto.Id, produto.Codigo, produto.Saldo));
        }

        var resposta = new DebitarSaldoResponseDto(resultado);

        // Persiste o registro de idempotência JUNTO com os débitos, na mesma
        // transação (SaveChanges) — ou tudo é gravado, ou nada é.
        if (!string.IsNullOrWhiteSpace(chaveIdempotencia))
        {
            _db.RegistrosIdempotencia.Add(new RegistroIdempotencia
            {
                Chave = chaveIdempotencia,
                ResultadoJson = JsonSerializer.Serialize(resposta)
            });
        }

        await _db.SaveChangesAsync();
        return resposta;
    }

    private static ProdutoResponseDto Map(Produto p) =>
        new(p.Id, p.Codigo, p.Descricao, p.Saldo);
}
