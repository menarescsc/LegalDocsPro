using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Application.Dtos;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    public class GetContractsWithPaginationQuery : IRequest<PagedResponse<ContractDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }
}