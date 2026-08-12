using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Application.Dtos;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    public class GetContractsWithPaginationQueryHandler : IRequestHandler<GetContractsWithPaginationQuery, PagedResponse<ContractDto>>
    {
        private readonly IContractRepository _contractRepository;

        public GetContractsWithPaginationQueryHandler(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<PagedResponse<ContractDto>> Handle(GetContractsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            // Llamamos al repo pasándole los parámetros
            var (items, totalCount) = await _contractRepository.GetPagedAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // Mapeamos de Entidades (Domain) a DTOs (Application)
            // Mapeamos usando los paréntesis () obligatorios del record
            var dtoList = items.Select(c => new ContractDto(
                c.Id,
                c.Title,
                c.Description,
                "",           // 4to parámetro (string vacío porque no existe en entidad)
                "",           // 5to parámetro (string vacío porque no existe en entidad)
                null,         // 6to parámetro (DateTime? nulo)
                c.CreatedAt   // 7mo parámetro (DateTime de la clase base)
            )).ToList();

            // Devolvemos nuestro objeto paginado
            return new PagedResponse<ContractDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}