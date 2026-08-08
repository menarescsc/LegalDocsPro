using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, int>
    {
        private readonly IContractRepository _repository;

        public CreateContractCommandHandler(IContractRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateContractCommand request, CancellationToken cancellationToken)
        {
            // 1. Instanciamos la entidad de dominio. 
            // Recuerda que el constructor ya valida las reglas de negocio básicas.
            var contract = new Contract(
                request.Title,
                request.Description,
                request.DocumentUrl,
                request.ExpirationDate);

            // 2. Guardamos en la base de datos
            await _repository.AddAsync(contract);
            await _repository.SaveChangesAsync();

            // 3. Retornamos el ID autogenerado
            return contract.Id;
        }
    }
}