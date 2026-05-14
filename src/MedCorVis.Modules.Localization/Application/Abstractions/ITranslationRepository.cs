namespace MedCorVis.Modules.Localization.Application.Abstractions;

using MedCorVis.Modules.Localization.Domain;

internal interface ITranslationRepository
{
    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetAllGroupedAsync(CancellationToken ct = default);
    Task<HashSet<(string Culture, string Key)>> GetAllKeysAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Translation>> GetAllAsync(string? culture, CancellationToken ct = default);
    Task<Translation?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string culture, string key, CancellationToken ct = default);
    Task<Translation> AddAsync(string culture, string key, string value, string? description, string createdBy, bool isSystemDefined = false, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}