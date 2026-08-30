using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Application.Dtos;
using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
}