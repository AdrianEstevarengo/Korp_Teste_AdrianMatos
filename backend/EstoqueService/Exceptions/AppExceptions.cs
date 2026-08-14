namespace EstoqueService.Exceptions;

/// <summary>Recurso não encontrado (mapeado para HTTP 404).</summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>Violação de regra de negócio, ex.: código duplicado ou saldo
/// insuficiente (mapeado para HTTP 409 Conflict).</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
