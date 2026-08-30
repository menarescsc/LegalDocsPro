using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Exceptions;

namespace LegalDocsPro.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        // EF Core requires a parameterless constructor
        protected Role() { }

        /// <summary>
        /// Creates a new role with required fields.
        /// </summary>
        public Role(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required.");

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            Status = 1; // Active
        }

        /// <summary>
        /// Updates the role information.
        /// </summary>
        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required.");

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Checks if this is the Admin role.
        /// </summary>
        public bool IsAdmin => Name.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }
}