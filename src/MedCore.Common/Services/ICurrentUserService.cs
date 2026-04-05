namespace MedCore.Common.Services;

public interface ICurrentUserService
{
    string UserId { get; }
    bool IsAuthenticated { get; }
}