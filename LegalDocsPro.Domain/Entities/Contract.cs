using LegalDocsPro.Domain.Common;
using LegalDocsPro.Domain.Enums;

namespace LegalDocsPro.Domain.Entities
{
    public class Contract : BaseEntity
    {
        // Propiedades privadas o protegidas para el set, obligando a usar constructores o métodos
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string DocumentUrl { get; private set; } = string.Empty;
        public ContractStatus Status { get; private set; }
        public DateTime? ExpirationDate { get; private set; }

        // 1. Constructor vacío requerido por Entity Framework Core
        protected Contract() { }

        // 2. Constructor rico para crear nuevos contratos
        public Contract(string title, string description, string documentUrl, DateTime? expirationDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("El título del contrato es obligatorio.");

            Title = title;
            Description = description;
            DocumentUrl = documentUrl;
            ExpirationDate = expirationDate;

            // Regla de negocio: Todo contrato nuevo nace como Borrador
            Status = ContractStatus.Draft;
        }

        // 3. Comportamientos (Reglas de negocio encapsuladas)
        public void SendToReview()
        {
            if (Status != ContractStatus.Draft)
                throw new InvalidOperationException("Solo los borradores pueden enviarse a revisión.");

            Status = ContractStatus.InReview;
        }

        public void Approve()
        {
            if (Status != ContractStatus.InReview)
                throw new InvalidOperationException("Solo los contratos en revisión pueden ser aprobados.");

            Status = ContractStatus.Approved;
        }

        public void Activate()
        {
            if (Status != ContractStatus.Approved)
                throw new InvalidOperationException("El contrato debe estar aprobado antes de activarse.");

            Status = ContractStatus.Active;
        }

        public void UpdateDetails(string title, string description)
        {
            if (Status == ContractStatus.Active || Status == ContractStatus.Expired)
                throw new InvalidOperationException("No se pueden modificar los detalles de un contrato activo o expirado.");

            Title = title;
            Description = description;
        }
    }
}
