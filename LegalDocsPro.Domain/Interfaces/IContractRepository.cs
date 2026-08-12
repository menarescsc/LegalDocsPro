using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Domain.Interfaces
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task AddAsync(Contract contract);
        void Update(Contract contract);
        // No ponemos 'Delete' porque en sistemas legales no se borra, se cambia el estado a 'Cancelado'.

        // Debajo de los métodos que ya tienes...
        Task<(IEnumerable<Contract> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm);

        Task SaveChangesAsync();


    }
}