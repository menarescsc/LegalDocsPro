using LegalDocsPro.Application.Dtos;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
    {
        private readonly IContractRepository _repository;

        // Inyectamos el repositorio
        public GetContractByIdQueryHandler(IContractRepository repository)
        {
            _repository = repository;
        }

        public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el contrato en la base de datos
            var contract = await _repository.GetByIdAsync(request.Id);

            // 2. Si no existe, devolvemos nulo (luego la API lo volverá un error 404)
            if (contract == null)
                return null;

            // 3. Mapeamos la Entidad de Dominio a nuestro DTO (Transformación)
            return new ContractDto(
                contract.Id,
                contract.Title,
                contract.Description,
                contract.ClientName,        // Cambiado de DocumentUrl a ClientName
                contract.DocumentUrl,        // Agregado: DocumentUrl
                contract.Status.ToString(), // Convertimos el enum a string
                contract.EffectiveDate,     // Agregado el parámetro faltante
                contract.ExpirationDate,
                contract.CreatedAt             // Agregado: CreatedAt
            );
        }
    }
}