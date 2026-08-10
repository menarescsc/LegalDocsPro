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

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificamos si el correo ya existe en la base de datos
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // En un caso real crearíamos una excepción personalizada (ej. DuplicateEmailException)
                // Por simplicidad, lanzaremos una excepción general que atrapará nuestro Middleware
                throw new Exception("El correo electrónico ya está registrado.");
            }

            // 2. Encriptamos la contraseña usando BCrypt
            // HashPassword genera una "sal" aleatoria automáticamente, haciéndolo muy seguro
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Creamos la entidad de dominio
            var newUser = new User(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.RoleId
            );

            // 4. Guardamos en la base de datos
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            return newUser.Id;
        }
    }
}