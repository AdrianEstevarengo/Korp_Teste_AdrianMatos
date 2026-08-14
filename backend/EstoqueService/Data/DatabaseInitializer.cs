using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Data;

/// <summary>
/// Cria o schema no banco na inicialização, com retry — o container do
/// Postgres pode ainda estar subindo quando o serviço inicia.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InicializarAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

        for (var tentativa = 1; tentativa <= 12; tentativa++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync();
                app.Logger.LogInformation("Banco de Estoque pronto.");
                return;
            }
            catch (Exception ex) when (tentativa < 12)
            {
                app.Logger.LogWarning(
                    "Banco ainda indisponível (tentativa {Tentativa}): {Msg}. Nova tentativa em 3s...",
                    tentativa, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
