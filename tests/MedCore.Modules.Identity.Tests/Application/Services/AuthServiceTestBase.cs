namespace MedCore.Modules.Identity.Tests.Application.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Application.Abstractions.Authentication;
using MedCore.Modules.Identity.Application.Abstractions.Email;
using MedCore.Modules.Identity.Application.Abstractions.Persistence;
using MedCore.Modules.Identity.Application.Services;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Identity.Domain.Tokens;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Identity.Tests.Helpers;

public abstract class AuthServiceTestBase
{
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IJwtTokenService JwtTokenService;
    protected readonly IRefreshTokenRepository RefreshTokenRepository;
    protected readonly IIdentityEmailService IdentityEmailService;
    protected readonly IIdentityUnitOfWork UnitOfWork;
    protected readonly IDbContextTransaction Transaction;
    protected readonly IAuthService Sut;
    protected readonly ICurrentCultureService CurrentCultureService;
    
    protected static readonly JwtSettings DefaultJwtSettings = new()
    {
        SecretKey  = "TesterJamesCapaoLongSecretKeyGoesLikeThis",
        Issuer     = "MedCore",
        Audience   = "MedCore",
        AccessTokenExpirationInMinutes = 15,
        RefreshTokenExpirationInDays   = 7
    };
    
    protected AuthServiceTestBase()
    {
        UserManager            = MockUserManager.Create();
        JwtTokenService        = Substitute.For<IJwtTokenService>();
        RefreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        IdentityEmailService   = Substitute.For<IIdentityEmailService>();
        UnitOfWork             = Substitute.For<IIdentityUnitOfWork>();
        Transaction            = Substitute.For<IDbContextTransaction>();
        
        CurrentCultureService = Substitute.For<ICurrentCultureService>();
        CurrentCultureService.Culture.Returns(SupportedCultures.Default);
        
        UnitOfWork
            .BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Transaction);
        
        JwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>())
            .Returns("access-token");

        JwtTokenService.GenerateRefreshToken()
            .Returns("raw-refresh-token");
        
        Sut = new AuthService(
            UserManager,
            CurrentCultureService,
            JwtTokenService,
            RefreshTokenRepository,
            IdentityEmailService,
            UnitOfWork,
            Options.Create(DefaultJwtSettings),
            NullLogger<AuthService>.Instance);
    }
    
    protected static ApplicationUser CreateUser(
        string email     = "jjcapaotest@softwareengineers.ch",
        bool isActive    = true,
        bool emailConfirmed = false)
    {
        var user = ApplicationUser.Create(
            email,
            "Jerald James Capao",
            "Test",
            new DateOnly(1988, 6, 27),
            ApplicationUser.SelfRegisteredActor);

        // EmailConfirmed has no setter — use reflection to set it for testing.
        typeof(ApplicationUser)
            .GetProperty(nameof(ApplicationUser.EmailConfirmed))!
            .SetValue(user, emailConfirmed);

        if (!isActive)
        {
            user.Deactivate(ApplicationUser.SelfRegisteredActor);
        }

        return user;
    }
}