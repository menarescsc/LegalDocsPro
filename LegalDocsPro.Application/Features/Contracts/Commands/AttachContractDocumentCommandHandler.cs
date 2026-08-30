using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Domain.Exceptions;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class AttachContractDocumentCommandHandler : IRequestHandler<AttachContractDocumentCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDomainEventDispatcher _eventDispatcher;

        private const string AdminRole = "Admin";

        public AttachContractDocumentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IDomainEventDispatcher eventDispatcher)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Result> Handle(AttachContractDocumentCommand request, CancellationToken cancellationToken)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);

            if (contract == null)
                return Result.Failure($"Contract with ID {request.ContractId} not found.", "NOT_FOUND");

            // Ownership check: non-admin users can only mutate their own contracts
            if (_currentUserService.Role != AdminRole && contract.CreatedBy != _currentUserService.UserId)
                return Result.Failure($"Contract with ID {request.ContractId} not found.", "NOT_FOUND");

            try
            {
                contract.AttachDocument(request.DocumentUrl);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _eventDispatcher.DispatchAllAsync(contract.DomainEvents, cancellationToken);
                contract.ClearDomainEvents();

                return Result.Success();
            }
            catch (DomainException ex)
            {
                return Result.Failure(ex.Message, "DOMAIN_ERROR");
            }
        }
    }
}