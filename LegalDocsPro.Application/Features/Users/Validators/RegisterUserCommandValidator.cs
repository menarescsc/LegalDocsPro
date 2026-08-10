using FluentValidation;
using LegalDocsPro.Application.Features.Users.Commands;

namespace LegalDocsPro.Application.Features.Users.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Debe asignar un rol válido al usuario.");
        }
    }
}