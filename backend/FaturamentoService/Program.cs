using FaturamentoService.Clients;
using FaturamentoService.Data;
using FaturamentoService.Middleware;
using FaturamentoService.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados: SQLite por padrão (roda local sem Docker) ou PostgreSQL.
// Selecione via configuração "Database:Provider" (Sqlite | Postgres).
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<FaturamentoDbContext>(options =>
{
    if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    else
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

// Cliente HTTP para o serviço de Estoque, com resiliência (Polly):
// - Retry: 3 tentativas com backoff exponencial para falhas transientes;
// - Circuit breaker: abre o circuito após 5 falhas seguidas por 15s.
var estoqueBaseUrl = builder.Configuration["Servicos:EstoqueBaseUrl"] ?? "http://localhost:5001";

builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(c =>
    {
        c.BaseAddress = new Uri(estoqueBaseUrl);
        c.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddPolicyHandler(RetryPolicy())
    .AddPolicyHandler(CircuitBreakerPolicy());

// API.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string CorsPolicy = "frontend";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy,
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsPolicy);
app.MapControllers();

await DatabaseInitializer.InicializarAsync(app);

app.Run();


// ----------------- Políticas de resiliência (Polly) -----------------

static IAsyncPolicy<HttpResponseMessage> RetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx, 408 e HttpRequestException
        .WaitAndRetryAsync(3, tentativa => TimeSpan.FromMilliseconds(300 * Math.Pow(2, tentativa - 1)));

static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15));
