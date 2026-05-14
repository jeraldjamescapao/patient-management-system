namespace MedCorVis.Modules.CodeItems.Application.Abstractions;

using MedCorVis.Modules.CodeItems.Domain;

internal interface ICodeItemRepository
{
    // Categories
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<Category?> GetCategoryByIdAsync(long id, CancellationToken ct = default);
    Task<Category?> GetCategoryByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CategoryCodeExistsAsync(string code, CancellationToken ct = default);
    Task AddCategoryAsync(Category category, CancellationToken ct = default);

    // Items
    Task<IReadOnlyList<CodeItem>> GetItemsByCategoryIdAsync(long categoryId, CancellationToken ct = default);
    Task<CodeItem?> GetItemByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ItemCodeExistsAsync(long categoryId, string code, CancellationToken ct = default);
    Task<bool> CategoryHasActiveItemsAsync(long categoryId, CancellationToken ct = default);
    Task AddItemAsync(CodeItem item, CancellationToken ct = default);

    // Translations
    Task<IReadOnlyList<CodeItemTranslation>> GetTranslationsByEntityAsync(
        string entityType, long entityId, CancellationToken ct = default);
    Task<CodeItemTranslation?> GetTranslationAsync(
        string entityType, long entityId, string culture, CancellationToken ct = default);
    Task AddTranslationAsync(CodeItemTranslation translation, CancellationToken ct = default);

    // Consumer
    Task<(Category? Category, IReadOnlyList<CodeItem> Items)> GetActiveByCategoryCodeAsync(
        string categoryCode, CancellationToken ct = default);
    Task<string?> GetLabelAsync(
        string entityType, long entityId, string culture, CancellationToken ct = default);
    
    // Persistence
    Task SaveChangesAsync(CancellationToken ct = default);
}