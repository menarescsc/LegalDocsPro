using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    // Usamos un record simple porque solo necesitamos el ID del contrato
    public record SendContractToReviewCommand(int Id) : IRequest<bool>;
}