namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using NSubstitute;
using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Application.Contracts.Authentication;
using MedCore.Modules.Identity.Domain.Users;
using Xunit;

public sealed class LoginTests : AuthServiceTestBase
{
    private static readonly LoginRequest ValidRequest = new(
        Email:    "jjcapaotest@softwareengineers.ch",
        Password: "ThePasswordOfAllSeasons!");
    
    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsUnauthorized()
    {
        UserManager.FindByEmailAsync(ValidRequest.Email).Returns((ApplicationUser?)null);

        var result = await Sut.LoginAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_INVALID_CREDENTIALS");
    }
    
    [Fact]
    public async Task LoginAsync_AccountDeactivated_ReturnsUnauthorized()
    {
        var user = CreateUser(ValidRequest.Email, isActive: false);
        UserManager.FindByEmailAsync(ValidRequest.Email).Returns(user);

        var result = await Sut.LoginAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_ACCOUNT_DEACTIVATED");
    }
    
    [Fact]
    public async Task LoginAsync_EmailNotConfirmed_ReturnsUnprocessableEntity()
    {
        var user = CreateUser(ValidRequest.Email, isActive: true, emailConfirmed: false);
        UserManager.FindByEmailAsync(ValidRequest.Email).Returns(user);

        var result = await Sut.LoginAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_EMAIL_NOT_CONFIRMED");
    }
    
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
    {
        var user = CreateUser(ValidRequest.Email, isActive: true, emailConfirmed: true);
        UserManager.FindByEmailAsync(ValidRequest.Email).Returns(user);
        UserManager.CheckPasswordAsync(user, ValidRequest.Password).Returns(false);

        var result = await Sut.LoginAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_INVALID_CREDENTIALS");
    }
    
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithCorrectShape()
    {
        var user = CreateUser(ValidRequest.Email, isActive: true, emailConfirmed: true);
        UserManager.FindByEmailAsync(ValidRequest.Email).Returns(user);
        UserManager.CheckPasswordAsync(user, ValidRequest.Password).Returns(true);
        UserManager.GetRolesAsync(user).Returns(["Patient"]);

        var result = await Sut.LoginAsync(ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(ValidRequest.Email);
        result.Value.FullName.Should().Be("Jerald James Capao Test");
        result.Value.Roles.Should().ContainSingle().Which.Should().Be("Patient");
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RawRefreshToken.Should().Be("raw-refresh-token");
    }
}