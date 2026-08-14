using System.Text.Json;
using FaturamentoService.Exceptions;

namespace FaturamentoService.Middleware;

/// <summary>
/// Tratamento global de erros/exceções do serviço de Faturamento.
/// Converte exceções em respostas HTTP padronizadas.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NaoEncontradoException ex)
        {
            await EscreverAsync(context, StatusCodes.Status404NotFound, "Não encontrado", ex.Message);
        }
        catch (RegraNegocioException ex)
        {
            await EscreverAsync(context, StatusCodes.Status409Conflict, "Conflito de regra de negócio", ex.Message);
        }
        catch (EstoqueIndisponivelException ex)
        {
            // Falha de microsserviço: o sistema se recupera (nota segue Aberta)
            // e devolve feedback claro ao usuário.
            _logger.LogWarning(ex, "Serviço de Estoque indisponível.");
            await EscreverAsync(context, StatusCodes.Status503ServiceUnavailable,
                "Serviço indisponível", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado.");
            await EscreverAsync(context, StatusCodes.Status500InternalServerError,
                "Erro interno", "Ocorreu um erro inesperado no serviço de Faturamento.");
        }
    }

    private static async Task EscreverAsync(HttpContext context, int status, string titulo, string detalhe)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var corpo = JsonSerializer.Serialize(new { status, titulo, detalhe });
        await context.Response.WriteAsync(corpo);
    }
}
