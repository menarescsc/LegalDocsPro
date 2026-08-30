using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Domain.Exceptions;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class ContractActivateCommandHandler : IRequestHandler<ContractActivateCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDomainEventDispatcher _eventDispatcher;

        private const string AdminRole = "Admin";

        public ContractActivateCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IDomainEventDispatcher eventDispatcher)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Result> Handle(ContractActivateCommand request, CancellationToken cancellationToken)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);

            if (contract == null)
                return Result.Failure($"Contract with ID {request.Id} not found.", "NOT_FOUND");

            // Ownership check: non-admin users can only mutate their own contracts
            if (_currentUserService.Role != AdminRole && contract.CreatedBy != _currentUserService.UserId)
                return Result.Failure($"Contract with ID {request.Id} not found.", "NOT_FOUND");

            try
            {
                contract.Activate();
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
