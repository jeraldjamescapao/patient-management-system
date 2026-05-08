namespace MedCore.Modules.Users.Tests.Application.Services.UserServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Users.Application.Contracts;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

public sealed class UpdateProfileTests : UserServiceTestBase
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly UpdateProfileRequest ValidRequest = new(
        FirstName: "James Capao",
        LastName: "Test Update",
        BirthDate: new DateOnly(1965, 10, 10));
    
    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNotFound()
    {
        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns((ApplicationUser?)null);

        var result = await Sut.UpdateProfileAsync(UserId, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("USERS_USER_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpdateProfileAsync_IdentityUpdateFails_ReturnsInternal()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .UpdateAsync(user)
            .Returns(IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateError",
                Description = "Update failed."
            }));

        var result = await Sut.UpdateProfileAsync(UserId, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Internal);
        result.Error!.Code.Should().Be("USERS_UPDATE_FAILED");
    }
    
    [Fact]
    public async Task UpdateProfileAsync_ValidRequest_ReturnsUpdatedProfile()
    {
        var user = CreateUser();

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        UserManager
            .UpdateAsync(user)
            .Returns(IdentityResult.Success);

        var result = await Sut.UpdateProfileAsync(UserId, ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("James Capao");
        result.Value.LastName.Should().Be("Test Update");
        result.Value.FullName.Should().Be("James Capao Test Update");
        result.Value.BirthDate.Should().Be(new DateOnly(1965, 10, 10));
    }
}