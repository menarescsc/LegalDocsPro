using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Files.Queries
{
    public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, Result<DownloadFileResult>>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        private const string AdminRole = "Admin";

        public DownloadFileQueryHandler(
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DownloadFileResult>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
            // Resolve file path
            var filePath = _fileStorageService.GetFilePath(request.StoredName);

            if (filePath == null)
                return Result<DownloadFileResult>.Failure("File not found.", "NOT_FOUND");

            // Find contract that references this file
            var contract = await FindContractByDocumentAsync(request.StoredName, cancellationToken);

            if (contract == null)
                return Result<DownloadFileResult>.Failure("File not found.", "NOT_FOUND");

            // Ownership check
            var userId = _currentUserService.UserId;
            var role = _currentUserService.Role;

            if (!string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(contract.CreatedBy, userId, StringComparison.Ordinal))
            {
                return Result<DownloadFileResult>.Failure("File not found.", "NOT_FOUND");
            }

            return Result<DownloadFileResult>.Success(new DownloadFileResult
            {
                FilePath = filePath,
                ContentType = "application/pdf"
            });
        }

        private async Task<Domain.Entities.Contract?> FindContractByDocumentAsync(string storedName, CancellationToken cancellationToken)
        {
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();

            return allContracts.FirstOrDefault(c =>
                !string.IsNullOrEmpty(c.DocumentUrl) &&
                (c.DocumentUrl.Contains(storedName) ||
                 c.DocumentUrl.EndsWith($"/{storedName}", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
