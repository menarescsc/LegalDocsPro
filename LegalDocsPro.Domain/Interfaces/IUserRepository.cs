using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Necesitaremos buscar por email más adelante para el Login
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}