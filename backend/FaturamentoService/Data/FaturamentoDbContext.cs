using FaturamentoService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> Notas => Set<NotaFiscal>();
    public DbSet<ItemNota> Itens => Set<ItemNota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(e =>
        {
            e.ToTable("notas_fiscais");
            e.HasKey(n => n.Id);
            e.Property(n => n.Numero).IsRequired();
            e.HasIndex(n => n.Numero).IsUnique();
            e.Property(n => n.Status).HasConversion<int>().IsRequired();
            e.Property(n => n.ChaveIdempotencia).HasMaxLength(200);

            e.HasMany(n => n.Itens)
                .WithOne(i => i.NotaFiscal!)
                .HasForeignKey(i => i.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNota>(e =>
        {
            e.ToTable("itens_nota");
            e.HasKey(i => i.Id);
            e.Property(i => i.Codigo).IsRequired().HasMaxLength(50);
            e.Property(i => i.Descricao).IsRequired().HasMaxLength(300);
            e.Property(i => i.Quantidade).IsRequired();
        });
    }
}
