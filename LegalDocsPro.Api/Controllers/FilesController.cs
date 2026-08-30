using LegalDocsPro.Application.Features.Files.Commands;
using LegalDocsPro.Application.Features.Files.Queries;
using MediatR;
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
        private readonly IMediator _mediator;

        public FilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request, CancellationToken ct)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            var command = new UploadFileCommand
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                FileStream = file.OpenReadStream()
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error, code = result.ErrorCode });

            return Ok(new { storedName = result.Value });
        }

        [HttpGet("download/{storedName}")]
        public async Task<IActionResult> DownloadFile(string storedName, CancellationToken ct)
        {
            var query = new DownloadFileQuery { StoredName = storedName };
            var result = await _mediator.Send(query, ct);

            if (result.IsFailure)
                return NotFound(new { error = result.Error, code = result.ErrorCode });

            var fileStream = new FileStream(result.Value!.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, result.Value.ContentType, storedName);
        }
    }
}
