using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Application.Common.Interfaces
{
    // Este es el contrato. La capa de aplicación solo sabe que si envía un Usuario, 
    // recibirá un string (el token JWT), sin importarle cómo se construye.
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}