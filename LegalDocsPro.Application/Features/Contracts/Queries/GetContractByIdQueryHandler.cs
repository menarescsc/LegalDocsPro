using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Dtos;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
    {
        private readonly IContractRepository _repository;
        private readonly ICurrentUserService _currentUserService;

        private const string AdminRole = "Admin";

        public GetContractByIdQueryHandler(IContractRepository repository, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(request.Id);

            if (contract == null)
                return null;

            // Ownership check: non-admin users can only access their own contracts
            if (_currentUserService.Role != AdminRole && contract.CreatedBy != _currentUserService.UserId)
                return null;

            return new ContractDto(
                contract.Id,
                contract.Title,
                contract.Description,
                contract.ClientName,
                contract.DocumentUrl,
                contract.Status.ToString(),
                contract.EffectiveDate,
                contract.ExpirationDate,
                contract.CreatedAt
            );
        }
    }
}