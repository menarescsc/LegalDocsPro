using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalDocsPro.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore domain events — they are not persisted
            modelBuilder.Ignore<DomainEvent>();

            // Apply all configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}