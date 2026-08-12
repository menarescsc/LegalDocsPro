using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalDocsPro.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        // Inyectamos ICurrentUserService en el constructor
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        // Sobrescribimos el método de guardado
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Recorremos todas las entidades que heredan de BaseEntity y que hayan sido modificadas o agregadas
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Usamos .Property().CurrentValue porque las propiedades tienen "protected set"
                        entry.Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow;
                        entry.Property(x => x.CreatedBy).CurrentValue = _currentUserService.UserId ?? "Sistema";
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.LastModifiedAt).CurrentValue = DateTime.UtcNow;
                        entry.Property(x => x.LastModifiedBy).CurrentValue = _currentUserService.UserId ?? "Sistema";
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}