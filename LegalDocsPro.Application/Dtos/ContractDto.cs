namespace LegalDocsPro.Application.Dtos
{
    // Usamos 'record' porque los DTOs solo transportan datos, no cambian.
    public record ContractDto(
        int Id,
        string Title,
        string Description,
        string DocumentUrl,
        string Status,
        DateTime? ExpirationDate,
        DateTime CreatedAt
    );
}