using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for User aggregate.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        void Update(User user);
    }
}