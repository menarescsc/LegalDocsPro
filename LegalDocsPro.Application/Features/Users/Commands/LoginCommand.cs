using LegalDocsPro.Application.Dtos;
using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    // El usuario envía su correo y contraseña, y espera a cambio un AuthResponseDto
    public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
}