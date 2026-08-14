namespace FaturamentoService.Clients;

// Contrato de comunicação com o serviço de Estoque (endpoint /api/produtos/debitar).

public record ItemDebitoDto(int ProdutoId, int Quantidade);

public record DebitarSaldoRequestDto(List<ItemDebitoDto> Itens);

public record ItemSaldoDto(int ProdutoId, string Codigo, int NovoSaldo);

public record DebitarSaldoResponseDto(List<ItemSaldoDto> Itens);

// Corpo de erro padronizado retornado pelo Estoque.
public record ErroDto(int Status, string Titulo, string Detalhe);
