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
    }
}