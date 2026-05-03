namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using MedCore.Common.Exceptions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

public sealed class EmailConfirmationAsyncTests : AuthServiceTestBase
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

public sealed class ResendConfirmationEmailAsyncTests : AuthServiceTestBase
{
    private const string Email = "jjcapaotest@softwareengineers.ch";
    
    [Fact]
    public async Task ResendConfirmationEmailAsync_UserNotFound_SilentlySucceeds()
    {
        UserManager
            .FindByEmailAsync(Email)
            .Returns((ApplicationUser?)null);

        var result = await Sut.ResendConfirmationEmailAsync(Email);

        result.IsSuccess.Should().BeTrue();
        await IdentityEmailService
            .DidNotReceive()
            .SendConfirmationEmailAsync(
                Arg.Any<ApplicationUser>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ResendConfirmationEmailAsync_EmailAlreadyConfirmed_SilentlySucceeds()
    {
        var user = CreateUser(emailConfirmed: true);

        UserManager.FindByEmailAsync(Email).Returns(user);

        var result = await Sut.ResendConfirmationEmailAsync(Email);

        result.IsSuccess.Should().BeTrue();
        await IdentityEmailService
            .DidNotReceive()
            .SendConfirmationEmailAsync(
                Arg.Any<ApplicationUser>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ResendConfirmationEmailAsync_EmailDeliveryFails_ReturnsServiceUnavailable()
    {
        var user = CreateUser(emailConfirmed: false);

        UserManager
            .FindByEmailAsync(Email)
            .Returns(user);
        
        UserManager
            .GenerateEmailConfirmationTokenAsync(user)
            .Returns("raw-token");
        
        IdentityEmailService
            .SendConfirmationEmailAsync(user, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new EmailDeliveryException("SMTP failed.")));

        var result = await Sut.ResendConfirmationEmailAsync(Email);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.ServiceUnavailable);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_EMAIL_DELIVERY_FAILED");
    }
    
    [Fact]
    public async Task ResendConfirmationEmailAsync_ValidRequest_ReturnsSuccess()
    {
        var user = CreateUser(emailConfirmed: false);

        UserManager
            .FindByEmailAsync(Email)
            .Returns(user);
        
        UserManager
            .GenerateEmailConfirmationTokenAsync(user)
            .Returns("raw-token");
        
        IdentityEmailService
            .SendConfirmationEmailAsync(user, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await Sut.ResendConfirmationEmailAsync(Email);

        result.IsSuccess.Should().BeTrue();
        await IdentityEmailService
            .Received(1)
            .SendConfirmationEmailAsync(user, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}