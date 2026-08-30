using FluentAssertions;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Features.Users.Commands;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using Moq;

namespace LegalDocsPro.Application.Tests.Features.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new RegisterUserCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAssignDefaultRole_IgnoringAnyClientInput()
    {
        // Arrange — command has no RoleId; handler must assign role 1
        var command = new RegisterUserCommand("Alice", "Smith", "alice@example.com", "SecureP@ss1");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — captured user must have RoleId = 1
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.RoleId == 1 &&
            u.Email == command.Email &&
            u.FirstName == "Alice" &&
            u.LastName == "Smith"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_BeforePersisting()
    {
        var command = new RegisterUserCommand("Bob", "Jones", "bob@example.com", "PlainTextPass1");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.PasswordHash != "PlainTextPass1" &&
            u.PasswordHash.StartsWith("$2") // BCrypt prefix
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var command = new RegisterUserCommand("Alice", "Smith", "existing@example.com", "SecureP@ss1");
        var existingUser = new User("Existing", "User", "existing@example.com", "hash", 1);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public void Command_ShouldNotHaveRoleId_Property()
    {
        // Verify the command record does not expose RoleId to callers
        var command = new RegisterUserCommand("Test", "User", "test@example.com", "pass123");

        var properties = typeof(RegisterUserCommand).GetProperties();
        properties.Should().NotContain(p => p.Name == "RoleId");
    }
}
