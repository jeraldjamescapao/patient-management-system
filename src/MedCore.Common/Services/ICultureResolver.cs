namespace MedCore.Common.Services;

public interface ICultureResolver
{
    Task<string> ResolveForUserAsync(Guid userId, CancellationToken ct = default);
}