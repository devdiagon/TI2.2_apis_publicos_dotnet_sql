using Microsoft.EntityFrameworkCore;

namespace ec.edu.monster.modelo;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Pais> Paises { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pais>(e =>
        {
            e.ToTable("Paises");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(Pais.LongitudMaxima);
            e.Property(p => p.Capital).HasMaxLength(Pais.LongitudMaxima);
            e.Property(p => p.Region).IsRequired().HasMaxLength(Pais.LongitudMaxima);
            e.Property(p => p.Subregion).HasMaxLength(Pais.LongitudMaxima);
            e.Property(p => p.Moneda).HasMaxLength(Pais.LongitudMaxima);
            e.HasIndex(p => p.Nombre).IsUnique().HasDatabaseName("UX_Paises_Nombre");
        });
    }
}
