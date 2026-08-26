using FluentAssertions;
using LegalDocsPro.Application.Features.Users.Commands;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using Moq;

namespace LegalDocsPro.Application.Tests.Features.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new RegisterUserCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAssignDefaultRole_IgnoringAnyClientInput()
    {
        // Arrange — command has no RoleId; handler must assign role 1
        var command = new RegisterUserCommand("Alice", "Smith", "alice@example.com", "SecureP@ss1");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        // Act
        var userId = await _handler.Handle(command, CancellationToken.None);

        // Assert — captured user must have RoleId = 1
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.RoleId == 1 &&
            u.Email == command.Email &&
            u.FirstName == "Alice" &&
            u.LastName == "Smith"
        )), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_BeforePersisting()
    {
        var command = new RegisterUserCommand("Bob", "Jones", "bob@example.com", "PlainTextPass1");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        await _handler.Handle(command, CancellationToken.None);

        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.PasswordHash != "PlainTextPass1" &&
            u.PasswordHash.StartsWith("$2") // BCrypt prefix
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        var command = new RegisterUserCommand("Alice", "Smith", "existing@example.com", "SecureP@ss1");
        var existingUser = new User("Existing", "User", "existing@example.com", "hash", 1);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
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
