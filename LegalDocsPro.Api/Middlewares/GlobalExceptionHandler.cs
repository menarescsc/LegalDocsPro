using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Dejamos un registro en la consola para el desarrollador
            _logger.LogError(exception, "Error atrapado por el GlobalExceptionHandler");

            // 2. Preparamos una respuesta estandarizada
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ha ocurrido un error inesperado. Por favor, intente más tarde."
            };

            // 3. Traducimos excepciones de negocio a códigos HTTP correctos
            if (exception is InvalidOperationException)
            {
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Error de validación o regla de negocio";
                problemDetails.Detail = exception.Message; // Aquí sí mostramos el error (ej: "El contrato ya está aprobado")
            }
            else if (exception is KeyNotFoundException)
            {
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Recurso no encontrado";
                problemDetails.Detail = exception.Message;
            }

            // 4. Enviamos la respuesta al cliente
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Le decimos a .NET: "Tranquilo, yo me encargo de este error, no colapses"
        }
    }
}