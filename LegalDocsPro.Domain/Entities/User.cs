using LegalDocsPro.Domain.Common;
using System.Net.NetworkInformation;

namespace LegalDocsPro.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        // Claves foráneas y propiedades de navegación para conectar con Role
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        // Constructor vacío requerido por Entity Framework Core
        private User() { }

        public User(string firstName, string lastName, string email, string passwordHash, int roleId)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            Status = 1; // 1 = Activo
        }
    }
}