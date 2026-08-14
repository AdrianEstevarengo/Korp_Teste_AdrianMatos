using System.Text;
using System.Text.Json;
using EstoqueService.DTOs;

namespace EstoqueService.Services;

/// <summary>
/// Gera a descrição de um produto a partir do código usando IA.
///
/// - Se a chave de API (Ia:ApiKey) estiver configurada, chama a API de chat da
///   OpenAI (compatível) para gerar a descrição.
/// - Caso contrário, opera em MODO SIMULADO (offline), montando uma descrição
///   plausível localmente. Assim o recurso funciona no teste mesmo sem chave.
/// </summary>
public class DescricaoIaService : IDescricaoIaService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<DescricaoIaService> _logger;

    public DescricaoIaService(HttpClient http, IConfiguration config, ILogger<DescricaoIaService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<GerarDescricaoResponseDto> GerarAsync(GerarDescricaoRequestDto request)
    {
        var apiKey = _config["Ia:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            return new GerarDescricaoResponseDto(GerarLocal(request), GeradoPorIa: false);

        try
        {
            var descricao = await GerarViaOpenAiAsync(request, apiKey);
            return new GerarDescricaoResponseDto(descricao, GeradoPorIa: true);
        }
        catch (Exception ex)
        {
            // Fallback resiliente: se a IA externa falhar, ainda entregamos algo útil.
            _logger.LogWarning(ex, "Falha ao chamar a IA externa. Usando geração local.");
            return new GerarDescricaoResponseDto(GerarLocal(request), GeradoPorIa: false);
        }
    }

    private async Task<string> GerarViaOpenAiAsync(GerarDescricaoRequestDto request, string apiKey)
    {
        var baseUrl = _config["Ia:BaseUrl"] ?? "https://api.openai.com/v1";
        var model = _config["Ia:Model"] ?? "gpt-4o-mini";

        var prompt =
            $"Gere uma descrição comercial curta (máx. 20 palavras), em português, para um produto " +
            $"de código '{request.Codigo}'." +
            (string.IsNullOrWhiteSpace(request.PalavrasChave)
                ? ""
                : $" Considere as palavras-chave: {request.PalavrasChave}.") +
            " Responda apenas com a descrição, sem aspas.";

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "Você é um assistente que cria descrições de produtos para um ERP." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 60
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
        msg.Headers.Add("Authorization", $"Bearer {apiKey}");
        msg.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(msg);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(json);
        var texto = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return (texto ?? "").Trim();
    }

    /// <summary>Geração local determinística usada quando não há chave de IA.</summary>
    private static string GerarLocal(GerarDescricaoRequestDto request)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var extras = string.IsNullOrWhiteSpace(request.PalavrasChave)
            ? "alta qualidade e ótimo custo-benefício"
            : request.PalavrasChave.Trim();

        return $"Produto {codigo} — item de {extras}, ideal para uso profissional e pronta entrega.";
    }
}
