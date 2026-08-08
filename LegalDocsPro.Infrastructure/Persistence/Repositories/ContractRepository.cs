using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using LegalDocsPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LegalDocsPro.Infrastructure.Persistence.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            // Usamos AsNoTracking() si solo vamos a leer, pero aquí podríamos necesitar modificar el estado luego
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}