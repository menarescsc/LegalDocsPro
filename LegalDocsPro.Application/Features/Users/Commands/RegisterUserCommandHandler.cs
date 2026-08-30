using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Exceptions;
using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        // Default role ID assigned to all self-registered users (prevents privilege escalation)
        private const int DefaultRoleId = 1;

        public RegisterUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Check if email already exists
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Result<int>.Failure("A user with this email address already exists.", "DUPLICATE_EMAIL");
                }

                // 2. Hash password using BCrypt (generates random salt automatically)
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 3. Create domain entity with server-assigned default role
                var newUser = new User(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    passwordHash,
                    DefaultRoleId
                );

                // 4. Persist to database
                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<int>.Success(newUser.Id);
            }
            catch (DomainException ex)
            {
                return Result<int>.Failure(ex.Message, "DOMAIN_ERROR");
            }
        }
    }
}