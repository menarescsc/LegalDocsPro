namespace LegalDocsPro.Domain.Common
{
    // Cambia 'internal' por 'public'
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int Status { get; protected set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}