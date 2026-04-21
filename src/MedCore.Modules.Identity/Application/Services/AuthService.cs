namespace MedCore.Modules.Identity.Application.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MedCore.Common.Exceptions;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Application.Abstractions.Authentication;
using MedCore.Modules.Identity.Application.Abstractions.Email;
using MedCore.Modules.Identity.Application.Abstractions.Persistence;
using MedCore.Modules.Identity.Application.Contracts.Authentication;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Identity.Domain.Roles;
using MedCore.Modules.Identity.Domain.Tokens;
using MedCore.Modules.Identity.Domain.Users;
using System.Security.Cryptography;
using System.Text;

internal sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IIdentityEmailService _identityEmailService;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IIdentityEmailService identityEmailService,
        IIdentityUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _identityEmailService = identityEmailService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }
    
    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<RegisterResponse>.Conflict(AuthErrors.EmailAlreadyRegistered);

        var user = ApplicationUser.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            createdBy: ApplicationUser.SelfRegisteredActor);
        
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync(ct);
                return Result<RegisterResponse>.Internal(AuthErrors.RegistrationFailed);
            }
            
            // Self-registration always assigns the Patient role.
            // Staff accounts are provisioned by administrators through a separate flow.
            var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.Patient);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(ct);
                return Result<RegisterResponse>.Internal(AuthErrors.RoleAssignmentFailed);
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, rawRefreshToken) = await IssueTokenPairAsync(user, roles, ct);
            
            var encodedToken = await GenerateEncodedConfirmationTokenAsync(user);
            await _identityEmailService.SendConfirmationEmailAsync(user, encodedToken, ct);
            
            await transaction.CommitAsync(ct);
            
            return Result<RegisterResponse>.Success(new RegisterResponse(
                user.Id,
                user.Email!,
                user.FullName,
                roles,
                accessToken)
            {
                RawRefreshToken = rawRefreshToken
            });
        }
        catch (EmailDeliveryException)
        {
            await transaction.RollbackAsync(ct);
            return Result<RegisterResponse>.ServiceUnavailable(AuthErrors.EmailDeliveryFailed);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<LoginResponse>.Unauthorized(AuthErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result<LoginResponse>.Unauthorized(AuthErrors.AccountDeactivated);

        if (!user.EmailConfirmed)
            return Result<LoginResponse>.UnprocessableEntity(AuthErrors.EmailNotConfirmed);

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result<LoginResponse>.Unauthorized(AuthErrors.InvalidCredentials);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, rawRefreshToken) = await IssueTokenPairAsync(user, roles, ct);
        
        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.Email!,
            user.FullName,
            roles,
            accessToken)
        {
            RawRefreshToken = rawRefreshToken
        });
    }

    public async Task<Result<RefreshResponse>> RefreshAsync(
        string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<RefreshResponse>.Unauthorized(AuthErrors.InvalidRefreshToken);

        var existingToken = await _refreshTokenRepository.GetByTokenAsync(HashToken(refreshToken), ct);
        if (existingToken is null)
            return Result<RefreshResponse>.Unauthorized(AuthErrors.InvalidRefreshToken);

        if (!existingToken.IsActive)
        {
            // A revoked token that was already replaced is a reuse signal.
            // Someone is replaying an old token — likely stolen (theft alert!).
            // Revoke the entire family to protect the user.
            if (existingToken.ReplacedByTokenId is not null)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(existingToken.UserId, ct);
                return Result<RefreshResponse>.Unauthorized(AuthErrors.TokenReuseDetected);
            }

            return Result<RefreshResponse>.Unauthorized(AuthErrors.TokenExpiredOrRevoked);
        }
        
        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null || !user.IsActive)
            return Result<RefreshResponse>.Unauthorized(AuthErrors.UserNotFoundOrInactive);

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var newRawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            existingToken.FamilyId,
            newRawRefreshToken,
            DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays));

        existingToken.Revoke();
        existingToken.MarkReplacedBy(newRefreshToken.Id);
        
        await _refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);
        
        return Result<RefreshResponse>.Success(new RefreshResponse(newAccessToken)
        {
            RawRefreshToken = newRawRefreshToken
        });
    }

    public async Task<Result<bool>> LogoutAsync(
        string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<bool>.Success(true);
        
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct);
        if (existingToken is null || !existingToken.IsActive)
            return Result<bool>.Success(true);
        
        existingToken.Revoke();
        await _refreshTokenRepository.SaveChangesAsync(ct);
        
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> LogoutAllAsync(
        Guid userId, CancellationToken ct = default)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ConfirmEmailAsync(
        Guid userId, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<bool>.NotFound(AuthErrors.UserNotFound);
        
        if (user.EmailConfirmed)
            return Result<bool>.Conflict(AuthErrors.EmailAlreadyConfirmed);
        
        var decodedToken = DecodeToken(token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            return Result<bool>.UnprocessableEntity(AuthErrors.InvalidConfirmationToken);
        
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResendConfirmationEmailAsync(
        string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // Always return success to avoid email enumeration attacks.
        if (user is null || user.EmailConfirmed)
            return Result<bool>.Success(true);
        
        try
        {
            var encodedToken = await GenerateEncodedConfirmationTokenAsync(user);
            await _identityEmailService.SendConfirmationEmailAsync(user, encodedToken, ct);
        }
        catch (EmailDeliveryException)
        {
            return Result<bool>.ServiceUnavailable(AuthErrors.EmailDeliveryFailed);
        }

        return Result<bool>.Success(true);
    }

    private async Task<(string AccessToken, string RawRefreshToken)> IssueTokenPairAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            user.Id,
            Guid.NewGuid(),
            HashToken(rawRefreshToken),
            DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays));

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return (accessToken, rawRefreshToken);
    }
    
    private async Task<string> GenerateEncodedConfirmationTokenAsync(ApplicationUser user)
    {
        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
    }

    private static string DecodeToken(string token)
    {
        var bytes = WebEncoders.Base64UrlDecode(token);
        return Encoding.UTF8.GetString(bytes);
    }
    
    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}