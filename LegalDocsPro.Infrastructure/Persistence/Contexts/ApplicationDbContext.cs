using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalDocsPro.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Representa nuestra tabla Contracts en SQL Server
        public DbSet<Contract> Contracts { get; set; }

        // Aquí interceptamos el guardado para llenar los campos de auditoría automáticamente
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = "System"; // Más adelante lo conectaremos con JWT
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = "System";
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        // Aquí configuramos Entity Framework para que entienda nuestro Dominio Rico
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("Contracts");
                entity.HasKey(e => e.Id);

                // Habilitamos las Temporal Tables de SQL Server (Auditoría automática)
                entity.ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.UseHistoryTable("ContractsHistory");
                    ttb.HasPeriodStart("ValidFrom");
                    ttb.HasPeriodEnd("ValidTo");
                }));

                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.DocumentUrl).HasMaxLength(500);
            });
        }
    }
}