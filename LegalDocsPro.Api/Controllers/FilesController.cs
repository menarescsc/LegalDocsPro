using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // Esta clase obliga a Swagger a mostrar el botón de archivo
    public class FileUploadRequest
    {
        public required IFormFile File { get; set; }
    }
    public class FilesController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FilesController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request) // <--- Ahora recibe la clase
        {
            var file = request.File; // <--- Extraemos el archivo aquí

            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No se proporcionó ningún archivo.");

                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                    return BadRequest("Solo se permiten archivos PDF.");

                var currentPath = Directory.GetCurrentDirectory();
                var uploadsFolder = Path.Combine(currentPath, "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var requestContext = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{requestContext.Scheme}://{requestContext.Host}";
                var fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";

                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al subir el archivo: {ex.Message}");
            }
        }
    }
}