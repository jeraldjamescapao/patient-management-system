namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

public sealed class ConfirmEmailTests : AuthServiceTestBase
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const string ValidToken = "dmFsaWQtdG9rZW4"; // Base64Url of "valid-token"
    
    [Fact]
    public async Task ConfirmEmailAsync_UserNotFound_ReturnsNotFound()
    {
        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns((ApplicationUser?)null);

        var result = await Sut.ConfirmEmailAsync(UserId, ValidToken);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_USER_NOT_FOUND");
    }
    
    [Fact]
    public async Task ConfirmEmailAsync_EmailAlreadyConfirmed_ReturnsConflict()
    {
        var user = CreateUser(emailConfirmed: true);

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);

        var result = await Sut.ConfirmEmailAsync(UserId, ValidToken);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_EMAIL_ALREADY_CONFIRMED");
    }
    
    [Fact]
    public async Task ConfirmEmailAsync_InvalidToken_ReturnsUnprocessableEntity()
    {
        var user = CreateUser(emailConfirmed: false);

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);
        
        UserManager
            .ConfirmEmailAsync(user, Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

        var result = await Sut.ConfirmEmailAsync(UserId, ValidToken);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_INVALID_CONFIRMATION_TOKEN");
    }
    
    [Fact]
    public async Task ConfirmEmailAsync_ValidToken_ReturnsSuccess()
    {
        var user = CreateUser(emailConfirmed: false);

        UserManager
            .FindByIdAsync(UserId.ToString())
            .Returns(user);
        
        UserManager
            .ConfirmEmailAsync(user, Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var result = await Sut.ConfirmEmailAsync(UserId, ValidToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}