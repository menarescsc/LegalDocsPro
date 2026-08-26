using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class SendContractToReviewCommandHandler : IRequestHandler<SendContractToReviewCommand, bool>
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICurrentUserService _currentUserService;

        private const string AdminRole = "Admin";

        public SendContractToReviewCommandHandler(IContractRepository contractRepository, ICurrentUserService currentUserService)
        {
            _contractRepository = contractRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(SendContractToReviewCommand request, CancellationToken cancellationToken)
        {
            var contract = await _contractRepository.GetByIdAsync(request.Id);

            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");

            // Ownership check: non-admin users can only mutate their own contracts
            if (_currentUserService.Role != AdminRole && contract.CreatedBy != _currentUserService.UserId)
                throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");

            contract.SendToReview();

            await _contractRepository.UpdateAsync(contract);

            return true;
        }
    }
}