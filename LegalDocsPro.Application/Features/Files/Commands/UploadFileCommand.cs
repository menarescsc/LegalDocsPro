using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Files.Commands
{
    public class UploadFileCommand : IRequest<Result<string>>
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
        public Stream FileStream { get; set; } = Stream.Null;
    }
}
