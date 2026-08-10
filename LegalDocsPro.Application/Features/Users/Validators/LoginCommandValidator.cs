using FluentValidation;
using LegalDocsPro.Application.Features.Users.Commands;

namespace LegalDocsPro.Application.Features.Users.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}