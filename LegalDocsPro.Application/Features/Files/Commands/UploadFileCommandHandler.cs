using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Files.Commands
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<string>>
    {
        private readonly IFileStorageService _fileStorageService;

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private const string PdfContentType = "application/pdf";
        private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();

        public UploadFileCommandHandler(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            // Validate file size
            if (request.Length > MaxFileSize)
                return Result<string>.Failure("File size exceeds the 10 MB limit.", "FILE_TOO_LARGE");

            // Validate content type
            if (!string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
                return Result<string>.Failure("Only PDF files are accepted.", "INVALID_CONTENT_TYPE");

            // Validate file extension
            if (!Path.GetExtension(request.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return Result<string>.Failure("Only PDF files are accepted.", "INVALID_EXTENSION");

            // Validate magic bytes
            if (!await HasPdfMagicBytesAsync(request.FileStream))
                return Result<string>.Failure("File content does not match PDF format.", "INVALID_FORMAT");

            // Reset stream position after magic bytes check
            request.FileStream.Position = 0;

            // Save file
            var storedName = await _fileStorageService.SaveAsync(request.FileStream, request.FileName, cancellationToken);

            return Result<string>.Success(storedName);
        }

        private static async Task<bool> HasPdfMagicBytesAsync(Stream stream)
        {
            if (stream.Length < PdfMagicBytes.Length)
                return false;

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
