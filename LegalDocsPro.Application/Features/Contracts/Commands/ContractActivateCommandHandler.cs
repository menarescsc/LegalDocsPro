using LegalDocsPro.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    internal class ContractActivateCommandHandler : IRequestHandler<ContractActivateCommand, bool>
    {
        private readonly IContractRepository _contractRepository;

        public ContractActivateCommandHandler(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<bool> Handle(ContractActivateCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el contrato
            var contract = await _contractRepository.GetByIdAsync(request.Id);

            if (contract == null)
                throw new KeyNotFoundException($"No se encontró el contrato con ID {request.Id}");

            // 2. Ejecutamos la regla de negocio de nuestra entidad (DDD puro)
            contract.Activate();

            // 3. Guardamos los cambios
            await _contractRepository.UpdateAsync(contract);

            return true;
        }
    }
}
