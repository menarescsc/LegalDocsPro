namespace LegalDocsPro.Application.Dtos
{
    // Usamos un record posicional limpio que coincide con la base de datos
    public record ContractDto(
        int Id,
        string Title,
        string Description,
        string ClientName,
        string Status,
        DateTime? EffectiveDate,
        DateTime? ExpirationDate,
        DateTime CreatedAt
    );
}