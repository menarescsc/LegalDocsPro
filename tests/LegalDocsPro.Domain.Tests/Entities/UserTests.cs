using FluentAssertions;
using LegalDocsPro.Domain.Entities;

namespace LegalDocsPro.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldInitializeUserPropertiesAndActiveStatus()
    {
        var user = new User(
            "Ada",
            "Lovelace",
            "ada@example.com",
            "hashed-password",
            2);

        user.FirstName.Should().Be("Ada");
        user.LastName.Should().Be("Lovelace");
        user.Email.Should().Be("ada@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.RoleId.Should().Be(2);
        user.Role.Should().BeNull();
        user.Status.Should().Be(1);
    }
}
