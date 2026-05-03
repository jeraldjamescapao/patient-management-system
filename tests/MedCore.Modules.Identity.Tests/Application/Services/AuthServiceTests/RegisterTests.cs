namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using Microsoft.AspNetCore.Identity;
using NSubstitute;
using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Common.Exceptions;
using MedCore.Modules.Identity.Application.Contracts.Authentication;
using MedCore.Modules.Identity.Domain.Users;
using Xunit;

public sealed class RegisterTests : AuthServiceTestBase
{
    private static readonly RegisterRequest ValidRequest = new(
        FirstName: "Jerald James Capao",
        LastName:  "Test",
        Email:     "jjcapaotest@softwareengineers.ch",
        Password:  "ThePasswordOfAllSeasons!",
        BirthDate: new DateOnly(1988, 6, 27));
    
    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsConflict()
    {
        UserManager
            .FindByEmailAsync(ValidRequest.Email)
            .Returns(CreateUser(ValidRequest.Email));

        var result = await Sut.RegisterAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_EMAIL_ALREADY_REGISTERED");
    }

    [Fact]
    public async Task RegisterAsync_UserCreationFails_ReturnsInternalError()
    {
        UserManager
            .FindByEmailAsync(ValidRequest.Email)
            .Returns((ApplicationUser?)null);
        
        UserManager
            .CreateAsync(Arg.Any<ApplicationUser>(), ValidRequest.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));
        
        var result = await Sut.RegisterAsync(ValidRequest);
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Internal);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_REGISTRATION_FAILED");
        await Transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_RoleAssignmentFails_ReturnInternalError()
    {
        UserManager
            .FindByEmailAsync(ValidRequest.Email)
            .Returns((ApplicationUser?)null);
        
        UserManager
            .CreateAsync(Arg.Any<ApplicationUser>(), ValidRequest.Password)
            .Returns(IdentityResult.Success);
        
        UserManager
            .AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));
        
        var result = await Sut.RegisterAsync(ValidRequest);
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Internal);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_ROLE_ASSIGNMENT_FAILED");
        await Transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_EmailDeliveryFails_ReturnServiceUnavailable()
    {
        UserManager
            .FindByEmailAsync(ValidRequest.Email)
            .Returns((ApplicationUser?)null);
        
        UserManager
            .CreateAsync(Arg.Any<ApplicationUser>(), ValidRequest.Password)
            .Returns(IdentityResult.Success);
        
        UserManager
            .AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        
        UserManager
            .GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(["Patient"]);
        
        UserManager
            .GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>())
            .Returns("raw-email-token");
        
        IdentityEmailService
            .SendConfirmationEmailAsync(
                Arg.Any<ApplicationUser>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new EmailDeliveryException("SMTP failed.")));
        
        var result = await Sut.RegisterAsync(ValidRequest);
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.ServiceUnavailable);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_EMAIL_DELIVERY_FAILED");
        await Transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccessWithCorrectShape()
    {
        UserManager
            .FindByEmailAsync(ValidRequest.Email)
            .Returns((ApplicationUser?)null);
        
        UserManager
            .CreateAsync(Arg.Any<ApplicationUser>(), ValidRequest.Password)
            .Returns(IdentityResult.Success);
        
        UserManager
            .AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        
        UserManager
            .GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(["Patient"]);
        
        UserManager
            .GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>())
            .Returns("raw-email-token");
        
        IdentityEmailService
            .SendConfirmationEmailAsync(
                Arg.Any<ApplicationUser>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var result = await Sut.RegisterAsync(ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(ValidRequest.Email);
        result.Value.FullName.Should().Be("Jerald James Capao Test");
        result.Value.Roles.Should().ContainSingle().Which.Should().Be("Patient");
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RawRefreshToken.Should().Be("raw-refresh-token");
        await Transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}