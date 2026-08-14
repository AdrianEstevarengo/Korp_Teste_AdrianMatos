using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaturamentoService.Exceptions;
using Polly.CircuitBreaker;

namespace FaturamentoService.Clients;

/// <summary>
/// Cliente HTTP para o serviço de Estoque. As políticas de resiliência
/// (retry + circuit breaker) são configuradas via Polly no Program.cs.
/// </summary>
public class EstoqueClient : IEstoqueClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<EstoqueClient> _logger;

    public EstoqueClient(HttpClient http, ILogger<EstoqueClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<DebitarSaldoResponseDto> DebitarAsync(
        DebitarSaldoRequestDto request, string chaveIdempotencia)
    {
        HttpResponseMessage resp;
        try
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/produtos/debitar")
            {
                Content = JsonContent.Create(request)
            };
            msg.Headers.Add("Idempotency-Key", chaveIdempotencia);

            resp = await _http.SendAsync(msg);
        }
        catch (BrokenCircuitException ex)
        {
            // O circuit breaker está aberto: o Estoque falhou repetidamente.
            _logger.LogWarning(ex, "Circuito aberto para o serviço de Estoque.");
            throw new EstoqueIndisponivelException(
                "O serviço de Estoque está temporariamente indisponível. Tente imprimir novamente em instantes.", ex);
        }
        catch (Exception ex) // HttpRequestException, TimeoutRejectedException, etc.
        {
            _logger.LogWarning(ex, "Falha de comunicação com o serviço de Estoque.");
            throw new EstoqueIndisponivelException(
                "Não foi possível contatar o serviço de Estoque. A nota permanece Aberta; tente novamente.", ex);
        }

        if (resp.IsSuccessStatusCode)
        {
            var ok = await resp.Content.ReadFromJsonAsync<DebitarSaldoResponseDto>(JsonOpts);
            return ok ?? new DebitarSaldoResponseDto(new());
        }

        // Erros de negócio retornados pelo Estoque não são "indisponibilidade":
        // repassamos a mensagem para o usuário.
        var detalhe = await LerDetalheErroAsync(resp);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new NaoEncontradoException(detalhe);

        if (resp.StatusCode == HttpStatusCode.Conflict)
            throw new RegraNegocioException(detalhe);

        // 5xx que escaparam do retry → tratamos como indisponibilidade.
        throw new EstoqueIndisponivelException(
            $"O serviço de Estoque respondeu com erro ({(int)resp.StatusCode}). Tente novamente.");
    }

    private static async Task<string> LerDetalheErroAsync(HttpResponseMessage resp)
    {
        try
        {
            var erro = await resp.Content.ReadFromJsonAsync<ErroDto>(JsonOpts);
            if (!string.IsNullOrWhiteSpace(erro?.Detalhe))
                return erro!.Detalhe;
        }
        catch
        {
            // ignora e cai no genérico
        }
        return "Erro ao processar a baixa de estoque.";
    }
}
