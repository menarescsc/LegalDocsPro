using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    /// <summary>
    /// Registration command. RoleId is intentionally excluded — the server assigns
    /// a fixed default role to all self-registered users to prevent privilege escalation.
    /// </summary>
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<Result<int>>;
}