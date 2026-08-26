using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Application.Dtos;
using LegalDocsPro.Domain.Interfaces;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    public class GetContractsWithPaginationQueryHandler : IRequestHandler<GetContractsWithPaginationQuery, PagedResponse<ContractDto>>
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICurrentUserService _currentUserService;

        private const string AdminRole = "Admin";

        public GetContractsWithPaginationQueryHandler(IContractRepository contractRepository, ICurrentUserService currentUserService)
        {
            _contractRepository = contractRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResponse<ContractDto>> Handle(GetContractsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            // Admin sees all contracts; non-admin sees only their own
            var isAdmin = _currentUserService.Role == AdminRole;
            var ownerId = isAdmin ? null : _currentUserService.UserId;

            var (items, totalCount) = await _contractRepository.GetPagedAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, ownerId);

            var dtoList = items.Select(c => new ContractDto(
                c.Id,
                c.Title,
                c.Description,
                c.ClientName,
                c.DocumentUrl,
                c.Status.ToString(),
                c.EffectiveDate,
                c.ExpirationDate,
                c.CreatedAt
            )).ToList();

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