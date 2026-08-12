using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class SendContractToReviewCommandHandler : IRequestHandler<SendContractToReviewCommand, bool>
    {
        private readonly IContractRepository _contractRepository;

        public SendContractToReviewCommandHandler(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<bool> Handle(SendContractToReviewCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el contrato
            var contract = await _contractRepository.GetByIdAsync(request.Id);

            if (contract == null)
                throw new KeyNotFoundException($"No se encontró el contrato con ID {request.Id}");

            // 2. Ejecutamos la regla de negocio de nuestra entidad (DDD puro)
            contract.SendToReview();

            // 3. Guardamos los cambios
            await _contractRepository.UpdateAsync(contract);

            return true;
        }
    }
}