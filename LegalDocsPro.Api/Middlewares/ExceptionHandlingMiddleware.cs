using System.Text.Json;
using FluentValidation;

namespace LegalDocsPro.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Dejamos que la petición fluya normalmente hacia los controladores
                await _next(context);
            }
            catch (ValidationException ex)
            {
                // Si nuestro guardia de seguridad (FluentValidation) lanza un error, lo atrapamos aquí
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                // Extraemos solo el nombre del campo y el mensaje de error
                var errors = ex.Errors.Select(e => new { Campo = e.PropertyName, Error = e.ErrorMessage });

                var result = JsonSerializer.Serialize(new
                {
                    Mensaje = "Se encontraron errores de validación.",
                    Errores = errors
                });

                await context.Response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                // Si ocurre cualquier OTRO error inesperado (ej. se cae la base de datos)
                _logger.LogError(ex, "Ocurrió un error no controlado en la aplicación.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var result = JsonSerializer.Serialize(new
                {
                    Mensaje = "Ocurrió un error interno en el servidor."
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}