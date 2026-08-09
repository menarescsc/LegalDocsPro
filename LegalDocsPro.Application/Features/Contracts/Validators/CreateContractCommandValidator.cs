using FluentValidation;
using LegalDocsPro.Application.Features.Contracts.Commands;

namespace LegalDocsPro.Application.Features.Contracts.Validators
{
    // Heredamos de AbstractValidator e indicamos qué comando vamos a validar
    public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
    {
        public CreateContractCommandValidator()
        {
            RuleFor(v => v.Title)
                .NotEmpty().WithMessage("El título del contrato es obligatorio.")
                .MaximumLength(200).WithMessage("El título no puede superar los 200 caracteres.");

            RuleFor(v => v.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

            RuleFor(v => v.ExpirationDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de expiración debe ser en el futuro.")
                .When(v => v.ExpirationDate.HasValue); // Solo validamos si enviaron una fecha
        }
    }
}