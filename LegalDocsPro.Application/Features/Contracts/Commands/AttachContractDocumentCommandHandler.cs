using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class AttachContractDocumentCommandHandler : IRequestHandler<AttachContractDocumentCommand, Unit>
    {
        private readonly IContractRepository _repository;
        private readonly ICurrentUserService _currentUserService;

        private const string AdminRole = "Admin";

        public AttachContractDocumentCommandHandler(IContractRepository repository, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(AttachContractDocumentCommand request, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(request.ContractId);

            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {request.ContractId} not found.");

            // Ownership check: non-admin users can only mutate their own contracts
            if (_currentUserService.Role != AdminRole && contract.CreatedBy != _currentUserService.UserId)
                throw new KeyNotFoundException($"Contract with ID {request.ContractId} not found.");

            contract.AttachDocument(request.DocumentUrl);

            await _repository.UpdateAsync(contract);

            return Unit.Value;
        }
    }
}