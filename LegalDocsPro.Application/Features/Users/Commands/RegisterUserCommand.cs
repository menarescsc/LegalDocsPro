using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    // El frontend enviará la contraseña en texto plano, nosotros nos encargaremos de encriptarla después.
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        int RoleId) : IRequest<int>;
}