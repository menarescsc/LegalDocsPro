using LegalDocsPro.Application.Common.Models;
using MediatR;

namespace LegalDocsPro.Application.Features.Files.Queries
{
    public class DownloadFileQuery : IRequest<Result<DownloadFileResult>>
    {
        public string StoredName { get; set; } = string.Empty;
    }

    public class DownloadFileResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";
    }
}
