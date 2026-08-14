namespace FaturamentoService.Exceptions;

/// <summary>Recurso não encontrado (HTTP 404).</summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>Violação de regra de negócio (HTTP 409), ex.: nota não está Aberta
/// ou saldo insuficiente reportado pelo Estoque.</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// O serviço de Estoque está indisponível (fora do ar / timeout / circuito aberto).
/// Mapeada para HTTP 503 para que o usuário receba feedback e possa tentar de novo.
/// </summary>
public class EstoqueIndisponivelException : Exception
{
    public EstoqueIndisponivelException(string mensagem, Exception? inner = null)
        : base(mensagem, inner) { }
}
