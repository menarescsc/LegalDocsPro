using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FilesController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")] // <--- 1. ESTO ES VITAL: Obliga a Swagger a formatear bien el archivo
        public async Task<IActionResult> UploadFile(IFormFile file) // <--- 2. Volvemos al parámetro simple
        {
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

                var request = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
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