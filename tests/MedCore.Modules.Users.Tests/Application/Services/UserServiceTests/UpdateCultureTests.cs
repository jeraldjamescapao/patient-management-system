namespace MedCore.Modules.Users.Tests.Application.Services.UserServiceTests;

using FluentAssertions;
using MedCore.Common.Localization;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

public sealed class UpdateCultureTests : UserServiceTestBase
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task UpdateCultureAsync_UnsupportedCulture_ReturnsValidation()
    {
        var result = await Sut.UpdateCultureAsync(UserId, "xxxyyyzzz");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error!.Code.Should().Be("USERS_UNSUPPORTED_CULTURE");
        await UserManager
            .DidNotReceive()
            .FindByIdAsync(Arg.Any<string>());
    }
    
    [Fact]
    public async Task UpdateCultureAsync_UserNotFound_ReturnsNotFound()
    {
        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns((ApplicationUser?)null);

        var result = await Sut.UpdateCultureAsync(UserId, SupportedCultures.French);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("USERS_USER_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpdateCultureAsync_ValidCulture_UpdatesAndSucceeds()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .UpdateAsync(user)
            .Returns(IdentityResult.Success);

        var result = await Sut.UpdateCultureAsync(UserId, SupportedCultures.French);

        result.IsSuccess.Should().BeTrue();
        user.PreferredCulture.Should().Be(SupportedCultures.French);
        await UserManager
            .Received(1)
            .UpdateAsync(user);
        UserCultureCache
            .Received(1)
            .InvalidateForUser(UserId);
    }
    
    [Fact]
    public async Task UpdateCultureAsync_RegionalCulture_AcceptsAndSucceeds()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .UpdateAsync(user)
            .Returns(IdentityResult.Success);

        var result = await Sut.UpdateCultureAsync(UserId, SupportedCultures.FrenchSwitzerland);

        result.IsSuccess.Should().BeTrue();
        user.PreferredCulture.Should().Be(SupportedCultures.FrenchSwitzerland);
    }
}