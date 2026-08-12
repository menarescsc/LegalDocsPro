using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, int>
    {
        private readonly IContractRepository _contractRepository;

        public CreateContractCommandHandler(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<int> Handle(CreateContractCommand request, CancellationToken cancellationToken)
        {
            // 1. Usamos el constructor rico que obliga a pasar los datos esenciales
            var contract = new Contract(
                request.Title,
                request.Description,
                request.DocumentUrl,
                request.ExpirationDate
            );

            // 2. Asignamos las propiedades adicionales
            contract.ClientName = request.ClientName;

            // 3. Guardamos en base de datos
            await _contractRepository.AddAsync(contract);

            return contract.Id;
        }
    }
}