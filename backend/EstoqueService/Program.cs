using EstoqueService.Data;
using EstoqueService.Middleware;
using EstoqueService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados (PostgreSQL via EF Core / Npgsql).
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Serviços de domínio.
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddHttpClient<IDescricaoIaService, DescricaoIaService>();

// API.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS liberado para o frontend Angular.
const string CorsPolicy = "frontend";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy,
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Tratamento global de erros/exceções.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsPolicy);
app.MapControllers();

// Cria/garante o schema no banco (com retry enquanto o Postgres sobe).
await DatabaseInitializer.InicializarAsync(app);

app.Run();
