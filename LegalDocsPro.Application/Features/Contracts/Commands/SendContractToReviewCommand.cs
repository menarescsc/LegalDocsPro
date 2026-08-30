using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public record SendContractToReviewCommand(int Id) : IRequest<Result>;
}