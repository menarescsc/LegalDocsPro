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
            // 1. Agrega la entidad a la memoria de seguimiento (Tracker)
            await _context.Contracts.AddAsync(contract);

            // 2. CONFIRMA LOS CAMBIOS EN SQL SERVER (Esta es la línea que suele faltar)
            await _context.SaveChangesAsync();

            // 3. Entity Framework actualiza automáticamente el contract.Id con el nuevo número
        }

        public void Update(Contract contract)
        {
            _context.Contracts.Update(contract);
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            // 1. Empezamos con la consulta base (sin ejecutarla aún)
            var query = _context.Contracts.AsQueryable();

            // 2. Aplicamos el filtro si el usuario escribió algo a buscar
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Busca coincidencias en el título o descripción
                query = query.Where(c => c.Title.Contains(searchTerm) || c.Description.Contains(searchTerm));
            }

            // 3. Contamos cuántos registros hay en total (después de filtrar)
            var totalCount = await query.CountAsync();

            // 4. Aplicamos la paginación (Skip y Take) y ordenamos por los más recientes
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}