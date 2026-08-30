using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class CreateContractCommand : IRequest<Result<int>>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
    }
}