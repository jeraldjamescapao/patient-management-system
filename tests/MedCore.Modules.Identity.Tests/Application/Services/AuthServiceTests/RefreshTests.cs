namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Tokens;
using MedCore.Modules.Identity.Domain.Users;
using NSubstitute;
using Xunit;

public sealed class RefreshTests : AuthServiceTestBase
{
    private static RefreshToken CreateActiveToken(Guid? userId = null) =>
        RefreshToken.Create(
            userId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "hashed-token",
            DateTimeOffset.UtcNow.AddDays(7));

    private static RefreshToken CreateRevokedToken(Guid? userId = null)
    {
        var token = RefreshToken.Create(
            userId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "hashed-revoked-token",
            DateTimeOffset.UtcNow.AddDays(7));

        token.Revoke(); // IsActive = false, ReplacedByTokenId = null → TokenExpiredOrRevoked path
        return token;
    }
    
    private static RefreshToken CreateExpiredToken(Guid? userId = null)
    {
        var token = RefreshToken.Create(
            userId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "hashed-expired-token",
            DateTimeOffset.UtcNow.AddSeconds(5)); // valid just long enough to pass the guard

        // Force ExpiresAtUtc into the past — IsExpired becomes true, IsActive becomes false.
        // IsRevoked remains false, so this is the pure expiry path (not the revoked path).
        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.ExpiresAtUtc))!
            .SetValue(token, DateTimeOffset.UtcNow.AddDays(-1));

        return token;
    }

    [Fact]
    public async Task RefreshAsync_EmptyToken_ReturnsUnauthorized()
    {
        var result = await Sut.RefreshAsync(string.Empty);
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_INVALID_REFRESH_TOKEN");
    }
    
    [Fact]
    public async Task RefreshAsync_TokenNotFound_ReturnsUnauthorized()
    {
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);
        
        var result = await Sut.RefreshAsync("some-raw-token");
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task RefreshAsync_TokenRevokedWithoutReplacement_ReturnsUnauthorized()
    {
        var token = CreateRevokedToken();
        
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);
        
        var result = await Sut.RefreshAsync("some-raw-token");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_TOKEN_EXPIRED_OR_REVOKED");
        await RefreshTokenRepository
            .DidNotReceive()
            .RevokeAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsUnauthorized()
    {
        var token = CreateExpiredToken();

        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);

        var result = await Sut.RefreshAsync("some-raw-token");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_TOKEN_EXPIRED_OR_REVOKED");
        await RefreshTokenRepository
            .DidNotReceive()
            .RevokeAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_TokenReuseDetected_RevokesAllAndReturnsUnauthorized()
    {
        var token = CreateActiveToken();

        // Revoked AND has a ReplacedByTokenId → reuse signal (stolen token replayed).
        token.Revoke();
        token.MarkReplacedBy(Guid.NewGuid());
        
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);
        
        var result = await Sut.RefreshAsync("some-raw-token");
        
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_TOKEN_REUSE_DETECTED");
        await RefreshTokenRepository
            .Received(1)
            .RevokeAllForUserAsync(token.UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_UserNotFoundOrInactive_ReturnsUnauthorized()
    {
        var token = CreateActiveToken();

        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);

        UserManager
            .FindByIdAsync(token.UserId.ToString())
            .Returns((ApplicationUser?)null);
        
        var result = await Sut.RefreshAsync("some-raw-token");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
        result.Error!.Code.Should().Be("IDENTITY_AUTH_USER_NOT_FOUND_OR_INACTIVE");
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsSuccessWithNewTokenPair()
    {
        var user = CreateUser(isActive: true, emailConfirmed: true);
        var token = CreateActiveToken(user.Id);
        
        RefreshTokenRepository
            .GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(token);
        
        UserManager
            .FindByIdAsync(user.Id.ToString())
            .Returns(user);
        
        UserManager
            .GetRolesAsync(user) 
            .Returns(["Patient"]);
        
        var result = await Sut.RefreshAsync("some-raw-token");
        
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RawRefreshToken.Should().Be("raw-refresh-token");
    }
}