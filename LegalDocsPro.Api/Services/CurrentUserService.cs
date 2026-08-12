using System.Security.Claims;
using LegalDocsPro.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LegalDocsPro.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Busca el ID del usuario dentro del Token JWT de la petición actual
        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}