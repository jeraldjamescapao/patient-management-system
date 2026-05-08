namespace MedCore.Common.Caching;

public interface IUserCultureCache
{
    void SetCultureForUser(Guid userId, string culture);
}