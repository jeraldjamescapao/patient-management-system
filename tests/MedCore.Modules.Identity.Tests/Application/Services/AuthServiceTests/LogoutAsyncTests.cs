namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using MedCore.Modules.Identity.Domain.Tokens;
using NSubstitute;
using Xunit;

public sealed class LogoutAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task LogoutAsync_EmptyToken_SilentlySucceeds()
    {
        var result = await Sut.LogoutAsync(string.Empty);

        result.IsSuccess.Should().BeTrue();
        await RefreshTokenRepository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task LogoutAsync_TokenNotFoundOrInactive_SilentlySucceeds()
    {
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var result = await Sut.LogoutAsync("some-raw-token");

        result.IsSuccess.Should().BeTrue();
        await RefreshTokenRepository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesTokenAndSucceeds()
    {
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hashed-token",
            DateTimeOffset.UtcNow.AddDays(7));
        
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);
        
        var result = await Sut.LogoutAsync("some-raw-token");
        
        result.IsSuccess.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
        await RefreshTokenRepository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public sealed class LogoutAllAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task LogoutAllAsync_ValidUserId_RevokesAllTokensAndSucceeds()
    {
        var userId = Guid.NewGuid();

        var result = await Sut.LogoutAllAsync(userId);

        result.IsSuccess.Should().BeTrue();
        await RefreshTokenRepository
            .Received(1)
            .RevokeAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}