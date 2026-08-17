using EstoqueService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<RegistroIdempotencia> RegistrosIdempotencia => Set<RegistroIdempotencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("produtos");
            e.HasKey(p => p.Id);
            e.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
            e.Property(p => p.Descricao).IsRequired().HasMaxLength(300);
            e.Property(p => p.Saldo).IsRequired();
            e.HasIndex(p => p.Codigo).IsUnique();

            // Controle de concorrência OTIMISTA compatível com SQLite e PostgreSQL:
            // um token (RowVersion) atualizado a cada gravação. UPDATEs concorrentes
            // sobre a mesma linha disparam DbUpdateConcurrencyException (tratada com retry).
            e.Property(p => p.RowVersion).IsConcurrencyToken();
        });

        modelBuilder.Entity<RegistroIdempotencia>(e =>
        {
            e.ToTable("registros_idempotencia");
            e.HasKey(r => r.Id);
            e.Property(r => r.Chave).IsRequired().HasMaxLength(200);
            e.Property(r => r.ResultadoJson).IsRequired();
            e.HasIndex(r => r.Chave).IsUnique();
        });
    }

    // Atualiza o token de concorrência sempre que um Produto é inserido ou alterado.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Produto>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.RowVersion = Guid.NewGuid();
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
