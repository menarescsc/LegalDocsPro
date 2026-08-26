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
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}