using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Contract aggregate.
    /// </summary>
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task AddAsync(Contract contract);
        void Update(Contract contract);

        Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm);

        /// <summary>
        /// Paged query with optional owner filter. When ownerId is non-null, only contracts
        /// created by that user are returned. When null, all contracts are returned (admin view).
        /// </summary>
        Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? ownerId);
    }
}