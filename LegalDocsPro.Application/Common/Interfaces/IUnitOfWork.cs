using LegalDocsPro.Domain.Interfaces;

namespace LegalDocsPro.Application.Common.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for transaction management.
    /// Coordinates multiple repository operations within a single transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Contract repository.
        /// </summary>
        IContractRepository Contracts { get; }

        /// <summary>
        /// User repository.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Saves all changes made within this unit of work.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
