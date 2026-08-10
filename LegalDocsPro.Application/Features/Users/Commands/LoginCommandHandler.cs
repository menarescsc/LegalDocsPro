using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Dtos;
using LegalDocsPro.Domain.Interfaces;
using MediatR;
using BCrypt.Net;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        // Inyectamos el repo y nuestra fábrica de tokens
        public LoginCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos al usuario por su correo
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Si el correo no existe, es un intento fallido
            if (user == null)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            // 2. Verificamos la contraseña con BCrypt
            // BCrypt entiende la "sal" que se usó al encriptar y verifica matemáticamente si coinciden
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            // 3. ¡Exito! Generamos el token JWT para este usuario
            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto(token, "Login exitoso.");
        }
    }
}