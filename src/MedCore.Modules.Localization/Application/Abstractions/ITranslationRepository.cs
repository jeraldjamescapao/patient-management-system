namespace MedCore.Modules.Localization.Application.Abstractions;

public interface ITranslationRepository
{
    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetAllGroupedAsync(CancellationToken ct = default);
    Task<HashSet<(string Culture, string Key)>> GetAllKeysAsync(CancellationToken ct = default);
    Task AddAsync(string culture, string key, string value, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}