namespace MedCore.Common.Caching;

public interface IUserCultureCache
{
    bool TryGetCultureForUser(Guid userId, out string? culture);
    void SetCultureForUser(Guid userId, string culture);
}