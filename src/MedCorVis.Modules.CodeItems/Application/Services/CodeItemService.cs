namespace MedCorVis.Modules.CodeItems.Application.Services;

using MedCorVis.Common.Localization;
using MedCorVis.Common.Results;
using MedCorVis.Common.Services;
using MedCorVis.Modules.CodeItems.Application.Abstractions;
using MedCorVis.Modules.CodeItems.Application.Contracts.Requests;
using MedCorVis.Modules.CodeItems.Application.Contracts.Responses;
using MedCorVis.Modules.CodeItems.Application.Errors;
using MedCorVis.Modules.CodeItems.Application.Logging;
using MedCorVis.Modules.CodeItems.Domain;
using Microsoft.Extensions.Logging;

internal sealed class CodeItemService : ICodeItemService
{
    private readonly ICodeItemRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentCultureService _currentCultureService;
    private readonly ILogger<CodeItemService> _logger;
    
    public CodeItemService(
        ICodeItemRepository repository,
        ICurrentUserService currentUserService,
        ICurrentCultureService currentCultureService,
        ILogger<CodeItemService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _currentCultureService = currentCultureService;
        _logger = logger;
    }
    
    #region Categories
    
    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetAllCategoriesAsync(
        CancellationToken ct = default)
    {
        var categories = await _repository.GetAllCategoriesAsync(ct);
        return Result<IReadOnlyList<CategoryResponse>>.Success(
            categories.Select(MapCategory).ToList());
    }
    
    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(
        long id, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(id, ct);
        
        if (category is not null) 
            return Result<CategoryResponse>.Success(MapCategory(category));
        
        CodeItemLogMessages.CategoryNotFoundById(_logger, id, null);
        return Result<CategoryResponse>.NotFound(CodeItemErrors.CategoryNotFound);
    }
    
    public async Task<Result<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await _repository.CategoryCodeExistsAsync(request.Code, ct))
        {
            CodeItemLogMessages.CategoryCodeAlreadyExists(_logger, request.Code, null);
            return Result<CategoryResponse>.Conflict(CodeItemErrors.CategoryCodeAlreadyExists);
        }

        var category = Category.Create(
            request.Code,
            request.Description,
            request.SortOrder,
            isSystemDefined: false,
            isEditable: true,
            isDeletable: true,
            _currentUserService.UserId);

        await _repository.AddCategoryAsync(category, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CategoryResponse>.Success(MapCategory(category));
    }
    
    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(
        long id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(id, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, id, null);
            return Result<CategoryResponse>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        if (!category.IsEditable)
            return Result<CategoryResponse>.UnprocessableEntity(CodeItemErrors.CategoryNotEditable);

        category.Update(request.Description, request.SortOrder, _currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<CategoryResponse>.Success(MapCategory(category));
    }
    
    public async Task<Result<bool>> ActivateCategoryAsync(
        long id, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(id, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        category.Activate(_currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> DeactivateCategoryAsync(
        long id, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(id, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        category.Deactivate(_currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> DeleteCategoryAsync(
        long id, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(id, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        if (!category.IsDeletable)
            return Result<bool>.UnprocessableEntity(CodeItemErrors.CategoryNotDeletable);

        if (await _repository.CategoryHasActiveItemsAsync(id, ct))
            return Result<bool>.UnprocessableEntity(CodeItemErrors.CategoryHasActiveItems);

        category.Delete(_currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> ReorderCategoriesAsync(
        ReorderRequest request, CancellationToken ct = default)
    {
        foreach (var entry in request.Entries)
        {
            var category = await _repository.GetCategoryByIdAsync(entry.Id, ct);
            category?.Update(category.Description, entry.SortOrder, _currentUserService.UserId);
        }

        await _repository.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    #endregion
    
    #region Items
    
    public async Task<Result<IReadOnlyList<ItemResponse>>> GetItemsByCategoryAsync(
        long categoryId, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, categoryId, null);
            return Result<IReadOnlyList<ItemResponse>>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        var items = await _repository.GetItemsByCategoryIdAsync(categoryId, ct);
        return Result<IReadOnlyList<ItemResponse>>.Success(
            items.Select(MapItem).ToList());
    }
    
    public async Task<Result<ItemResponse>> GetItemByIdAsync(
        long categoryId, long id, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(id, ct);

        if (item is not null && item.CategoryId == categoryId) 
            return Result<ItemResponse>.Success(MapItem(item));
        
        CodeItemLogMessages.ItemNotFound(_logger, id, null);
        return Result<ItemResponse>.NotFound(CodeItemErrors.ItemNotFound);
    }
    
    public async Task<Result<ItemResponse>> CreateItemAsync(
        long categoryId, CreateItemRequest request, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, categoryId, null);
            return Result<ItemResponse>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        if (await _repository.ItemCodeExistsAsync(categoryId, request.Code, ct))
        {
            CodeItemLogMessages.ItemCodeAlreadyExists(_logger, request.Code, categoryId, null);
            return Result<ItemResponse>.Conflict(CodeItemErrors.ItemCodeAlreadyExists);
        }

        var item = CodeItem.Create(
            categoryId,
            request.Code,
            request.Description,
            request.SortOrder,
            isSystemDefined: false,
            isEditable: true,
            isDeletable: true,
            _currentUserService.UserId);

        await _repository.AddItemAsync(item, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<ItemResponse>.Success(MapItem(item));
    }
    
    public async Task<Result<ItemResponse>> UpdateItemAsync(
        long categoryId, long id, UpdateItemRequest request, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(id, ct);

        if (item is null || item.CategoryId != categoryId)
        {
            CodeItemLogMessages.ItemNotFound(_logger, id, null);
            return Result<ItemResponse>.NotFound(CodeItemErrors.ItemNotFound);
        }

        if (!item.IsEditable)
            return Result<ItemResponse>.UnprocessableEntity(CodeItemErrors.ItemNotEditable);

        item.Update(request.Description, request.SortOrder, _currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<ItemResponse>.Success(MapItem(item));
    }
    
    public async Task<Result<bool>> ActivateItemAsync(
        long categoryId, long id, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(id, ct);

        if (item is null || item.CategoryId != categoryId)
        {
            CodeItemLogMessages.ItemNotFound(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.ItemNotFound);
        }

        item.Activate(_currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> DeactivateItemAsync(
        long categoryId, long id, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(id, ct);

        if (item is null || item.CategoryId != categoryId)
        {
            CodeItemLogMessages.ItemNotFound(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.ItemNotFound);
        }

        item.Deactivate(_currentUserService.UserId);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> DeleteItemAsync(
        long categoryId, long id, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(id, ct);

        if (item is null || item.CategoryId != categoryId)
        {
            CodeItemLogMessages.ItemNotFound(_logger, id, null);
            return Result<bool>.NotFound(CodeItemErrors.ItemNotFound);
        }

        if (!item.IsDeletable)
            return Result<bool>.UnprocessableEntity(CodeItemErrors.ItemNotDeletable);

        item.Delete(_currentUserService.UserId);

        // Cascade: delete all translations for this item
        var translations = await _repository.GetTranslationsByEntityAsync(
            CodeItemTranslation.EntityTypeItem, id, ct);

        foreach (var translation in translations)
            translation.Delete(_currentUserService.UserId);

        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<bool>> ReorderItemsAsync(
        long categoryId, ReorderRequest request, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, categoryId, null);
            return Result<bool>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        foreach (var entry in request.Entries)
        {
            var item = await _repository.GetItemByIdAsync(entry.Id, ct);
            item?.Update(item.Description, entry.SortOrder, _currentUserService.UserId);
        }

        await _repository.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    #endregion
    
    #region Translations
    
    public async Task<Result<IReadOnlyList<TranslationResponse>>> GetCategoryTranslationsAsync(
        long categoryId, CancellationToken ct = default)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFoundById(_logger, categoryId, null);
            return Result<IReadOnlyList<TranslationResponse>>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        var translations = await _repository.GetTranslationsByEntityAsync(
            CodeItemTranslation.EntityTypeCategory, categoryId, ct);

        return Result<IReadOnlyList<TranslationResponse>>.Success(
            translations.Select(MapTranslation).ToList());
    }
    
    public async Task<Result<TranslationResponse>> UpsertCategoryTranslationAsync(
        long categoryId, string culture, UpsertTranslationRequest request,
        CancellationToken ct = default)
    {
        if (!SupportedCultures.All.Contains(culture))
            return Result<TranslationResponse>.Validation(CodeItemErrors.UnsupportedCulture);

        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        
        if (category is not null)
            return await UpsertTranslationAsync(
                CodeItemTranslation.EntityTypeCategory, categoryId, culture, request, ct);
        
        CodeItemLogMessages.CategoryNotFoundById(_logger, categoryId, null);
        return Result<TranslationResponse>.NotFound(CodeItemErrors.CategoryNotFound);
    }
    
    public async Task<Result<IReadOnlyList<TranslationResponse>>> GetItemTranslationsAsync(
        long categoryId, long itemId, CancellationToken ct = default)
    {
        var item = await _repository.GetItemByIdAsync(itemId, ct);

        if (item is null || item.CategoryId != categoryId)
        {
            CodeItemLogMessages.ItemNotFound(_logger, itemId, null);
            return Result<IReadOnlyList<TranslationResponse>>.NotFound(CodeItemErrors.ItemNotFound);
        }

        var translations = await _repository.GetTranslationsByEntityAsync(
            CodeItemTranslation.EntityTypeItem, itemId, ct);

        return Result<IReadOnlyList<TranslationResponse>>.Success(
            translations.Select(MapTranslation).ToList());
    }
    
    public async Task<Result<TranslationResponse>> UpsertItemTranslationAsync(
        long categoryId, long itemId, string culture, UpsertTranslationRequest request,
        CancellationToken ct = default)
    {
        if (!SupportedCultures.All.Contains(culture))
            return Result<TranslationResponse>.Validation(CodeItemErrors.UnsupportedCulture);

        var item = await _repository.GetItemByIdAsync(itemId, ct);

        if (item is not null && item.CategoryId == categoryId)
            return await UpsertTranslationAsync(
                CodeItemTranslation.EntityTypeItem, itemId, culture, request, ct);
        
        CodeItemLogMessages.ItemNotFound(_logger, itemId, null);
        return Result<TranslationResponse>.NotFound(CodeItemErrors.ItemNotFound);
    }
    
    #endregion
    
    #region Consumer
    
    public async Task<Result<CodeItemListResponse>> GetActiveItemsAsync(
        string categoryCode, CancellationToken ct = default)
    {
        var (category, items) = 
            await _repository.GetActiveByCategoryCodeAsync(categoryCode, ct);

        if (category is null)
        {
            CodeItemLogMessages.CategoryNotFound(_logger, categoryCode, null);
            return Result<CodeItemListResponse>.NotFound(CodeItemErrors.CategoryNotFound);
        }

        var culture = _currentCultureService.Culture;

        var entries = new List<CodeItemEntry>();

        foreach (var item in items)
        {
            var label = await _repository.GetLabelAsync(
                CodeItemTranslation.EntityTypeItem, item.Id, culture, ct);

            // Fallback chain: requested culture → English → Code
            if (label is null && culture != SupportedCultures.English)
                label = await _repository.GetLabelAsync(
                    CodeItemTranslation.EntityTypeItem, item.Id, SupportedCultures.English, ct);

            entries.Add(new CodeItemEntry(item.Code, label ?? item.Code));
        }

        return Result<CodeItemListResponse>.Success(
            new CodeItemListResponse(category.Code, entries));
    }
    
    #endregion
    
    #region Private Helpers
    
    private async Task<Result<TranslationResponse>> UpsertTranslationAsync(
        string entityType, long entityId, string culture,
        UpsertTranslationRequest request, CancellationToken ct)
    {
        var existing = await _repository.GetTranslationAsync(entityType, entityId, culture, ct);

        if (existing is not null)
        {
            if (!existing.IsActive)
                existing.Reactivate(_currentUserService.UserId);
            
            existing.Update(request.Label, request.Description, _currentUserService.UserId);
            await _repository.SaveChangesAsync(ct);
            return Result<TranslationResponse>.Success(MapTranslation(existing));
        }

        var translation = CodeItemTranslation.Create(
            entityType,
            entityId,
            culture,
            request.Label,
            request.Description,
            isSystemDefined: false,
            _currentUserService.UserId);

        await _repository.AddTranslationAsync(translation, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<TranslationResponse>.Success(MapTranslation(translation));
    }

    private static CategoryResponse MapCategory(Category c)
    {
        return new CategoryResponse(
            c.Id, 
            c.Code, 
            c.Description, 
            c.IsActive, 
            c.IsSystemDefined,
            c.IsEditable, 
            c.IsDeletable, 
            c.IsDeleted, 
            c.SortOrder,
            c.CreatedAtUtc, 
            c.CreatedBy, 
            c.ModifiedAtUtc, 
            c.ModifiedBy);
    }
    
    private static ItemResponse MapItem(CodeItem i)
    {
        return new ItemResponse(
            i.Id, 
            i.CategoryId, 
            i.Code, 
            i.Description, 
            i.IsActive, 
            i.IsSystemDefined,
            i.IsEditable, 
            i.IsDeletable, 
            i.IsDeleted, 
            i.SortOrder,
            i.CreatedAtUtc, 
            i.CreatedBy, 
            i.ModifiedAtUtc, 
            i.ModifiedBy);
    }

    private static TranslationResponse MapTranslation(CodeItemTranslation t)
    {
        return new TranslationResponse(
            t.Id, 
            t.EntityType, 
            t.EntityId, 
            t.Culture, 
            t.Label, 
            t.Description,
            t.IsSystemDefined, 
            t.IsActive, 
            t.CreatedAtUtc, 
            t.CreatedBy,
            t.ModifiedAtUtc, 
            t.ModifiedBy);
    }
    
    #endregion
}