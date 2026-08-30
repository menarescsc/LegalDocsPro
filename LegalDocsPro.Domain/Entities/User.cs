using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Exceptions;

namespace LegalDocsPro.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        // Foreign key and navigation property for Role
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        // EF Core requires a parameterless constructor
        protected User() { }

        /// <summary>
        /// Creates a new user with required fields.
        /// </summary>
        public User(string firstName, string lastName, string email, string passwordHash, int roleId)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required.");
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required.");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("Password hash is required.");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            RoleId = roleId;
            Status = 1; // Active
        }

        /// <summary>
        /// Returns the full name of the user.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Updates user profile information.
        /// </summary>
        public void UpdateProfile(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required.");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        /// <summary>
        /// Changes the user's role.
        /// </summary>
        public void ChangeRole(int roleId)
        {
            if (roleId <= 0)
                throw new DomainException("Invalid role ID.");

            RoleId = roleId;
        }

        /// <summary>
        /// Deactivates the user account.
        /// </summary>
        public void Deactivate()
        {
            Status = 0; // Inactive
        }

        /// <summary>
        /// Activates the user account.
        /// </summary>
        public void Activate()
        {
            Status = 1; // Active
        }

        /// <summary>
        /// Checks if the user is active.
        /// </summary>
        public bool IsActive => Status == 1;
    }
}