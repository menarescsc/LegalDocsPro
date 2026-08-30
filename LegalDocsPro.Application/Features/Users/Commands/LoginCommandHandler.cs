using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Common.Models;
using LegalDocsPro.Application.Dtos;
using MediatR;

namespace LegalDocsPro.Application.Features.Users.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null)
                return Result<AuthResponseDto>.Failure("Invalid credentials.", "UNAUTHORIZED");

            // 2. Verify password with BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return Result<AuthResponseDto>.Failure("Invalid credentials.", "UNAUTHORIZED");

            // 3. Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(user);

            return Result<AuthResponseDto>.Success(new AuthResponseDto(token, "Login successful."));
        }
    }
}