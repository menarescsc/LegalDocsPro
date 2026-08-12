using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public record ContractActivateCommand(int Id) : IRequest<bool>;
}
