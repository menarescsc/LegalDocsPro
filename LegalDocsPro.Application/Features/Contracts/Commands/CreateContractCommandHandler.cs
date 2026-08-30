using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Exceptions;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public CreateContractCommandHandler(
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher eventDispatcher)
        {
            _unitOfWork = unitOfWork;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Result<int>> Handle(CreateContractCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Create domain entity using rich constructor
                var contract = new Contract(
                    request.Title,
                    request.Description,
                    request.ClientName,
                    request.DocumentUrl,
                    request.ExpirationDate
                );

                // 2. Persist to database
                await _unitOfWork.Contracts.AddAsync(contract);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 3. Dispatch domain events
                await _eventDispatcher.DispatchAllAsync(contract.DomainEvents, cancellationToken);
                contract.ClearDomainEvents();

                return Result<int>.Success(contract.Id);
            }
            catch (DomainException ex)
            {
                return Result<int>.Failure(ex.Message, "DOMAIN_ERROR");
            }
        }
    }
}