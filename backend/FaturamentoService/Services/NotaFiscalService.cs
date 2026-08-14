using FaturamentoService.Clients;
using FaturamentoService.Data;
using FaturamentoService.Domain;
using FaturamentoService.DTOs;
using FaturamentoService.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Services;

public class NotaFiscalService : INotaFiscalService
{
    private readonly FaturamentoDbContext _db;
    private readonly IEstoqueClient _estoque;
    private readonly ILogger<NotaFiscalService> _logger;

    public NotaFiscalService(FaturamentoDbContext db, IEstoqueClient estoque, ILogger<NotaFiscalService> logger)
    {
        _db = db;
        _estoque = estoque;
        _logger = logger;
    }

    public async Task<IEnumerable<NotaFiscalResponseDto>> ListarAsync()
    {
        var notas = await _db.Notas
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();

        return notas.Select(Map);
    }

    public async Task<NotaFiscalResponseDto> ObterAsync(int id)
    {
        var nota = await CarregarAsync(id);
        return Map(nota);
    }

    public async Task<NotaFiscalResponseDto> CriarAsync(CriarNotaDto dto)
    {
        // Numeração sequencial. LINQ: MAX(Numero) traduzido para SQL.
        var ultimoNumero = await _db.Notas.MaxAsync(n => (int?)n.Numero) ?? 0;

        var nota = new NotaFiscal
        {
            Numero = ultimoNumero + 1,
            Status = StatusNota.Aberta,
            DataCriacao = DateTime.UtcNow,
            Itens = dto.Itens.Select(i => new ItemNota
            {
                ProdutoId = i.ProdutoId,
                Codigo = i.Codigo,
                Descricao = i.Descricao,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _db.Notas.Add(nota);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nota {Numero} criada com {Qtd} item(ns).", nota.Numero, nota.Itens.Count);
        return Map(nota);
    }

    public async Task<NotaFiscalResponseDto> ImprimirAsync(int id)
    {
        var nota = await CarregarAsync(id);

        // Regra: só é permitido imprimir notas com status Aberta.
        if (nota.Status != StatusNota.Aberta)
            throw new RegraNegocioException(
                $"Não é possível imprimir a nota {nota.Numero}: o status atual é '{nota.Status}'. " +
                "Apenas notas Abertas podem ser impressas.");

        // Gera a chave de idempotência UMA vez e persiste. Reenvios (após falha
        // do Estoque) reutilizam a mesma chave, evitando baixa dupla no estoque.
        if (string.IsNullOrWhiteSpace(nota.ChaveIdempotencia))
        {
            nota.ChaveIdempotencia = Guid.NewGuid().ToString("N");
            await _db.SaveChangesAsync();
        }

        // Baixa no estoque (pode lançar EstoqueIndisponivelException ou
        // RegraNegocioException — em ambos os casos a nota permanece Aberta).
        var request = new DebitarSaldoRequestDto(
            nota.Itens.Select(i => new ItemDebitoDto(i.ProdutoId, i.Quantidade)).ToList());

        await _estoque.DebitarAsync(request, nota.ChaveIdempotencia!);

        // Sucesso: fecha a nota.
        nota.Status = StatusNota.Fechada;
        nota.DataImpressao = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nota {Numero} impressa e fechada.", nota.Numero);
        return Map(nota);
    }

    private async Task<NotaFiscal> CarregarAsync(int id)
    {
        return await _db.Notas.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new NaoEncontradoException($"Nota fiscal {id} não encontrada.");
    }

    private static NotaFiscalResponseDto Map(NotaFiscal n) => new(
        n.Id,
        n.Numero,
        n.Status.ToString(),
        n.DataCriacao,
        n.DataImpressao,
        n.Itens.Select(i => new ItemNotaResponseDto(i.ProdutoId, i.Codigo, i.Descricao, i.Quantidade)).ToList());
}
