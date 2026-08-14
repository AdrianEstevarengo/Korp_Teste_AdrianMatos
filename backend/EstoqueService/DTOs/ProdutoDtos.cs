using System.ComponentModel.DataAnnotations;

namespace EstoqueService.DTOs;

public record ProdutoCreateDto(
    [Required, MaxLength(50)] string Codigo,
    [Required, MaxLength(300)] string Descricao,
    [Range(0, int.MaxValue)] int Saldo);

public record ProdutoUpdateDto(
    [Required, MaxLength(300)] string Descricao,
    [Range(0, int.MaxValue)] int Saldo);

public record ProdutoResponseDto(int Id, string Codigo, string Descricao, int Saldo);

// ----- Débito de saldo (chamado pelo serviço de Faturamento na impressão) -----

public record ItemDebitoDto(
    [Range(1, int.MaxValue)] int ProdutoId,
    [Range(1, int.MaxValue)] int Quantidade);

public record DebitarSaldoRequestDto([Required] List<ItemDebitoDto> Itens);

public record ItemSaldoDto(int ProdutoId, string Codigo, int NovoSaldo);

public record DebitarSaldoResponseDto(List<ItemSaldoDto> Itens);

// ----- IA: geração de descrição -----

public record GerarDescricaoRequestDto(
    [Required, MaxLength(50)] string Codigo,
    string? PalavrasChave);

public record GerarDescricaoResponseDto(string Descricao, bool GeradoPorIa);
