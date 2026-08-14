using System.ComponentModel.DataAnnotations;

namespace FaturamentoService.DTOs;

public record ItemNotaCreateDto(
    [Range(1, int.MaxValue)] int ProdutoId,
    [Required, MaxLength(50)] string Codigo,
    [Required, MaxLength(300)] string Descricao,
    [Range(1, int.MaxValue)] int Quantidade);

public record CriarNotaDto(
    [Required, MinLength(1)] List<ItemNotaCreateDto> Itens);

public record ItemNotaResponseDto(int ProdutoId, string Codigo, string Descricao, int Quantidade);

public record NotaFiscalResponseDto(
    int Id,
    int Numero,
    string Status,
    DateTime DataCriacao,
    DateTime? DataImpressao,
    List<ItemNotaResponseDto> Itens);
