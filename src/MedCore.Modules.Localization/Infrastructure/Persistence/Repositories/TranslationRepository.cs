namespace MedCore.Modules.Localization.Infrastructure.Persistence.Repositories;

using MedCore.Modules.Localization.Application.Abstractions;
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
            .Select(t => new { t.Culture, t.Key, t.Value })
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
    
    public async Task<IReadOnlyList<Translation>> GetAllAsync(
        string? culture, CancellationToken ct = default)
    {
        var query = _context.Translations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(culture))
            query = query.Where(t => t.Culture == culture);

        return await query
            .OrderBy(t => t.Culture)
            .ThenBy(t => t.Key)
            .ToListAsync(ct);
    }
    
    public async Task<Translation?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Translations.FindAsync([id], ct);
    }
    
    public async Task<bool> ExistsAsync(string culture, string key, CancellationToken ct = default)
    {
        return await _context.Translations
            .AsNoTracking()
            .AnyAsync(t => t.Culture == culture && t.Key == key, ct);
    }

    public async Task<Translation> AddAsync(
        string culture, 
        string key, 
        string value, 
        string? description,
        string createdBy,
        CancellationToken ct = default)
    {
        var translation = Translation.Create(
            culture, 
            key, 
            value, 
            createdBy, 
            description);
        
        await _context.Translations.AddAsync(translation, ct);
        
        return translation; // EF populates Id after SaveChangesAsync is called
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}