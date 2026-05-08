namespace MedCore.Modules.Users.Tests.Application.Services.UserServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Users.Application.Contracts;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

public sealed class UpdatePhoneTests : UserServiceTestBase
{
    private static readonly Guid UserId = Guid.NewGuid();
    
    [Fact]
    public async Task UpdatePhoneAsync_UserNotFound_ReturnsNotFound()
    {
        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns((ApplicationUser?)null);

        var result = await Sut.UpdatePhoneAsync(UserId, "+41 79 123 45 67");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("USERS_USER_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpdatePhoneAsync_IdentityUpdateFails_ReturnsInternal()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .SetPhoneNumberAsync(user, Arg.Any<string?>())
            .Returns(IdentityResult.Failed(new IdentityError
            {
                Code = "PhoneError",
                Description = "Phone update failed."
            }));

        var result = await Sut.UpdatePhoneAsync(UserId, "+41 79 123 45 67");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Internal);
        result.Error!.Code.Should().Be("USERS_PHONE_UPDATE_FAILED");
    }
    
    [Fact]
    public async Task UpdatePhoneAsync_ValidPhone_Succeeds()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .SetPhoneNumberAsync(user, "+41 79 123 45 67")
            .Returns(IdentityResult.Success);

        var result = await Sut.UpdatePhoneAsync(UserId, "+41 79 123 45 67");

        result.IsSuccess.Should().BeTrue();
        await UserManager
            .Received(1)
            .SetPhoneNumberAsync(user, "+41 79 123 45 67");
    }
}