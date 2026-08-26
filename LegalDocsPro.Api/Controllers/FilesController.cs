using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    public class FileUploadRequest
    {
        public required IFormFile File { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IContractRepository _contractRepository;
        private readonly ICurrentUserService _currentUserService;

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private const string PdfContentType = "application/pdf";
        private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();

        public FilesController(
            IFileStorageService fileStorageService,
            IContractRepository contractRepository,
            ICurrentUserService currentUserService)
        {
            _fileStorageService = fileStorageService;
            _contractRepository = contractRepository;
            _currentUserService = currentUserService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request, CancellationToken ct)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            // Size validation: 10 MB max
            if (file.Length > MaxFileSize)
                return BadRequest("File size exceeds the 10 MB limit.");

            // Extension validation
            if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are accepted.");

            // Content-Type validation
            if (!string.Equals(file.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are accepted. Invalid content type.");

            // Magic bytes validation: file must start with %PDF
            if (!await HasPdfMagicBytesAsync(file))
                return BadRequest("File content does not match PDF format.");

            // Save via storage service (GUID filename, outside wwwroot)
            var storedName = await _fileStorageService.SaveAsync(file.OpenReadStream(), file.FileName, ct);

            return Ok(new { storedName });
        }

        [HttpGet("download/{storedName}")]
        public async Task<IActionResult> DownloadFile(string storedName, CancellationToken ct)
        {
            // Resolve file path from private storage
            var filePath = _fileStorageService.GetFilePath(storedName);

            if (filePath == null)
                return NotFound();

            // Ownership check: find a contract that references this file
            var contract = await FindContractByDocumentAsync(storedName, ct);

            if (contract == null)
                return NotFound();

            var userId = _currentUserService.UserId;
            var role = _currentUserService.Role;

            // Non-admin users can only download their own contract documents
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(contract.CreatedBy, userId, StringComparison.Ordinal))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, PdfContentType, $"{storedName}");
        }

        /// <summary>
        /// Finds a contract whose DocumentUrl references the given stored name.
        /// Supports both new private storage names and legacy /uploads/ paths.
        /// </summary>
        private async Task<Domain.Entities.Contract?> FindContractByDocumentAsync(string storedName, CancellationToken ct)
        {
            // Try to find by stored name (new storage format)
            // We need to search contracts — for now, check all contracts that might reference this file.
            // The storedName is the GUID-based filename. Contracts store DocumentUrl which may be
            // a legacy /uploads/ path or the storedName itself.
            var allContracts = await _contractRepository.GetAllAsync();

            return allContracts.FirstOrDefault(c =>
                !string.IsNullOrEmpty(c.DocumentUrl) &&
                (c.DocumentUrl.Contains(storedName) ||
                 c.DocumentUrl.EndsWith($"/{storedName}", StringComparison.OrdinalIgnoreCase)));
        }

        private static async Task<bool> HasPdfMagicBytesAsync(IFormFile file)
        {
            if (file.Length < PdfMagicBytes.Length)
                return false;

            using var stream = file.OpenReadStream();
            var buffer = new byte[PdfMagicBytes.Length];
            var bytesRead = await stream.ReadAsync(buffer);

            if (bytesRead < PdfMagicBytes.Length)
                return false;

            for (int i = 0; i < PdfMagicBytes.Length; i++)
            {
                if (buffer[i] != PdfMagicBytes[i])
                    return false;
            }

            return true;
        }
    }
}
