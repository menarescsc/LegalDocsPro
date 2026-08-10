namespace LegalDocsPro.Application.Dtos
{
    // Solo devolveremos el token y un mensaje
    public record AuthResponseDto(string Token, string Message);
}