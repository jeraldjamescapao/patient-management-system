namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using MedCore.Common.Exceptions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using NSubstitute;
using Xunit;

public sealed class ResendConfirmationEmailTests : AuthServiceTestBase
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