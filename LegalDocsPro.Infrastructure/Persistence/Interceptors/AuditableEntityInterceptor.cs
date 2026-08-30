using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LegalDocsPro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor that automatically sets audit fields on entities.
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly TimeProvider _dateTime;

        public AuditableEntityInterceptor(
            ICurrentUserService currentUserService,
            TimeProvider dateTime)
        {
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = _dateTime.GetUtcNow().UtcDateTime;
                    entry.Entity.CreatedBy = _currentUserService.UserId ?? "System";
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedAt = _dateTime.GetUtcNow().UtcDateTime;
                    entry.Entity.LastModifiedBy = _currentUserService.UserId ?? "System";
                }
            }
        }
    }
}
