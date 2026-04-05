namespace MedCore.Modules.Identity.Application.Abstractions.Authentication;

using MedCore.Common.Results;
using MedCore.Modules.Identity.Application.Contracts.Authentication;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<RefreshResponse>> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<bool>> LogoutAllAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default);
    Task<Result<bool>> ResendConfirmationEmailAsync(string email, CancellationToken ct = default);
}