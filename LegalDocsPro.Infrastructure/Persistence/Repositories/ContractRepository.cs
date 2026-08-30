using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LegalDocsPro.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for Contract aggregate.
    /// </summary>
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts.FindAsync(id);
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _context.Contracts.ToListAsync();
        }

        public async Task AddAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
        }

        public void Update(Contract contract)
        {
            _context.Contracts.Update(contract);
        }

        public async Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            return await GetPagedAsync(pageNumber, pageSize, searchTerm, ownerId: null);
        }

        public async Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? ownerId)
        {
            var query = _context.Contracts.AsQueryable();

            // Filter by owner when specified (non-admin users)
            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                query = query.Where(c => c.CreatedBy == ownerId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm) || c.Description.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}