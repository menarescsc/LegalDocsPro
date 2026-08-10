using LegalDocsPro.Domain.Common;
using System.Net.NetworkInformation;

namespace LegalDocsPro.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        // Constructor vacío requerido por Entity Framework Core
        private Role() { }

        public Role(string name, string description)
        {
            // Aquí podríamos agregar validaciones de dominio (ej. que el nombre no esté vacío)
            Name = name;
            Description = description;

            // Asumimos que 1 es Activo para nuestra clase base
            Status = 1;
        }
    }
}