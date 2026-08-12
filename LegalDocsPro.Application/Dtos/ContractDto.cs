namespace LegalDocsPro.Application.Dtos
{
    public record ContractDto(
        int Id,
        string Title,
        string Description,
        string ClientName,
        string DocumentUrl, // <--- Agregamos esto
        string Status,
        DateTime? EffectiveDate,
        DateTime? ExpirationDate,
        DateTime CreatedAt
    );
}