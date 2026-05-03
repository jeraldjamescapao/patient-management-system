namespace MedCore.Modules.Identity.Tests.Application.Services.AuthServiceTests;

using FluentAssertions;
using NSubstitute;
using Xunit;

public sealed class LogoutAllTests : AuthServiceTestBase
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