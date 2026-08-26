using LegalDocsPro.Application.Features.Users.Commands;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using MediatR;
using BCrypt.Net;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly IUserRepository _userRepository;

        // Default role ID assigned to all self-registered users (prevents privilege escalation)
        private const int DefaultRoleId = 1;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email address already exists.");
            }

            // 2. Hash password using BCrypt (generates random salt automatically)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Create domain entity with server-assigned default role
            var newUser = new User(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                DefaultRoleId
            );

            // 4. Persist to database
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            return newUser.Id;
        }
    }
}