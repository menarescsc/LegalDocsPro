using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    // IRequest<int> significa que este comando, al finalizar, devolverá el ID (int) del nuevo contrato.
    public record CreateContractCommand(
        string Title,
        string Description,
        string DocumentUrl,
        DateTime? ExpirationDate) : IRequest<int>;
}