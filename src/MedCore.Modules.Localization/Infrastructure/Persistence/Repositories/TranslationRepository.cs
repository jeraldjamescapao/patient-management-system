namespace MedCore.Modules.Localization.Infrastructure.Persistence.Repositories;

using MedCore.Common.Authorization;
using MedCore.Common.Localization;
using MedCore.Modules.Localization.Domain;
using Microsoft.EntityFrameworkCore;

internal sealed class TranslationRepository : ITranslationRepository
{
    private readonly LocalizationDbContext _context;

    public TranslationRepository(LocalizationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetAllGroupedAsync(
        CancellationToken ct = default)
    {
        var all = await _context.Translations
            .AsNoTracking()
            .Where(t => t.IsActive)
            .ToListAsync(ct);

        return all
            .GroupBy(t => t.Culture)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, string>)
                    g.ToDictionary(t => t.Key, t => t.Value));
    }
    
    public async Task<HashSet<(string Culture, string Key)>> GetAllKeysAsync(
        CancellationToken ct = default)
    {
        var pairs = await _context.Translations
            .AsNoTracking()
            .Select(t => new { t.Culture, t.Key })
            .ToListAsync(ct);

        return pairs.Select(p => (p.Culture, p.Key)).ToHashSet();
    }

    public async Task AddAsync(
        string culture, 
        string key, 
        string value,
        CancellationToken ct = default)
    {
        await _context.Translations.AddAsync(
            new Translation(culture, key, value, SystemActors.System), ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}