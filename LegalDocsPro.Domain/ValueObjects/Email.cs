using System.Text.RegularExpressions;
using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Exceptions;

namespace LegalDocsPro.Domain.ValueObjects
{
    /// <summary>
    /// Email Value Object. Immutable and self-validating.
    /// </summary>
    public class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new Email instance with validation.
        /// </summary>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email cannot be empty.");

            var normalized = email.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(normalized))
                throw new DomainException($"Invalid email format: {email}");

            return new Email(normalized);
        }

        /// <summary>
        /// Creates an Email instance from persistence (no validation).
        /// </summary>
        public static Email FromPersistence(string value)
        {
            return new Email(value);
        }

        public string Domain => Value.Split('@')[1];

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;
    }
}
