namespace MedCore.Modules.Users.Tests.Application.Services.UserServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using NSubstitute;
using Xunit;

public sealed class GetCurrentUserTests : UserServiceTestBase
{
    private static readonly Guid UserId = Guid.NewGuid();
    
    [Fact]
    public async Task GetCurrentUserAsync_UserNotFound_ReturnsNotFound()
    {
        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns((ApplicationUser?)null);

        var result = await Sut.GetCurrentUserAsync(UserId);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("USERS_USER_NOT_FOUND");
    }
    
    [Fact]
    public async Task GetCurrentUserAsync_UserExists_ReturnsCorrectShape()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        var result = await Sut.GetCurrentUserAsync(UserId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("jjcapaotest@softwareengineers.ch");
        result.Value.FirstName.Should().Be("Jerald James Capao");
        result.Value.LastName.Should().Be("Test");
        result.Value.FullName.Should().Be("Jerald James Capao Test");
        result.Value.BirthDate.Should().Be(new DateOnly(1988, 6, 27));
        result.Value.Culture.Should().BeNull();
        result.Value.IsActive.Should().BeTrue();
    }
}