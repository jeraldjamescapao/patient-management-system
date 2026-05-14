namespace MedCorVis.Modules.Localization.Application.Services;

using MedCorVis.Common.Localization;
using MedCorVis.Common.Results;
using MedCorVis.Common.Services;
using MedCorVis.Modules.Localization.Application.Abstractions;
using MedCorVis.Modules.Localization.Application.Contracts.Requests;
using MedCorVis.Modules.Localization.Application.Contracts.Responses;
using MedCorVis.Modules.Localization.Application.Errors;
using MedCorVis.Modules.Localization.Application.Logging;
using Microsoft.Extensions.Logging;

internal sealed class TranslationService : ITranslationService
{
    private readonly ITranslationRepository _repository;
    private readonly ILocalizerCache _localizerCache;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TranslationService> _logger;
    
    public TranslationService(
        ITranslationRepository repository,
        ILocalizerCache localizerCache,
        ICurrentUserService currentUserService,
        ILogger<TranslationService> logger)
    {
        _repository = repository;
        _localizerCache = localizerCache;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    public async Task<Result<IReadOnlyList<TranslationResponse>>> GetAllAsync(
        string? culture, CancellationToken ct = default)
    {
        if (culture is not null && !SupportedCultures.All.Contains(culture))
            return Result<IReadOnlyList<TranslationResponse>>.Validation(TranslationErrors.UnsupportedCulture);

        var translations = await _repository.GetAllAsync(culture, ct);
        return Result<IReadOnlyList<TranslationResponse>>.Success(
            translations.Select(MapToResponse).ToList());
    }

    public async Task<Result<TranslationResponse>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var translation = await _repository.GetByIdAsync(id, ct);

        if (translation is not null) 
            return Result<TranslationResponse>.Success(MapToResponse(translation));
        
        TranslationLogMessages.TranslationNotFound(_logger, id, null);
        return Result<TranslationResponse>.NotFound(TranslationErrors.NotFound);
    }
    
    public async Task<Result<TranslationResponse>> CreateAsync(
        CreateTranslationRequest request, CancellationToken ct = default)
    {
        if (!SupportedCultures.All.Contains(request.Culture))
            return Result<TranslationResponse>.Validation(TranslationErrors.UnsupportedCulture);

        var exists = await _repository.ExistsAsync(request.Culture, request.Key, ct);
        if (exists)
        {
            TranslationLogMessages.TranslationDuplicateKey(_logger, request.Culture, request.Key, null);
            return Result<TranslationResponse>.Conflict(TranslationErrors.DuplicateKey);
        }
        
        var translation = await _repository.AddAsync(
            request.Culture, 
            request.Key, 
            request.Value, 
            request.Description,
            _currentUserService.UserId, 
            false,
            ct);

        try
        {
            await _repository.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            TranslationLogMessages.TranslationCreateFailed(_logger, request.Culture, request.Key, ex);
            return Result<TranslationResponse>.Internal(TranslationErrors.CreateFailed);
        }

        await ReloadCacheAsync(ct);
        
        return Result<TranslationResponse>.Success(MapToResponse(translation));
    }
    
    public async Task<Result<TranslationResponse>> UpdateAsync(
        long id, UpdateTranslationRequest request, CancellationToken ct = default)
    {
        var translation = await _repository.GetByIdAsync(id, ct);

        if (translation is null)
        {
            TranslationLogMessages.TranslationNotFound(_logger, id, null);
            return Result<TranslationResponse>.NotFound(TranslationErrors.NotFound);
        }

        translation.Update(request.Value, request.Description, _currentUserService.UserId);

        try
        {
            await _repository.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            TranslationLogMessages.TranslationUpdateFailed(_logger, id, ex);
            return Result<TranslationResponse>.Internal(TranslationErrors.UpdateFailed);
        }

        await ReloadCacheAsync(ct);
        
        return Result<TranslationResponse>.Success(MapToResponse(translation));
    }
    
    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var translation = await _repository.GetByIdAsync(id, ct);

        if (translation is null)
        {
            TranslationLogMessages.TranslationNotFound(_logger, id, null);
            return Result<bool>.NotFound(TranslationErrors.NotFound);
        }

        translation.Deactivate(_currentUserService.UserId);

        try
        {
            await _repository.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            TranslationLogMessages.TranslationDeleteFailed(_logger, id, ex);
            return Result<bool>.Internal(TranslationErrors.DeleteFailed);
        }

        await ReloadCacheAsync(ct);
        
        return Result<bool>.Success(true);
    }
    
    public async Task RefreshCacheAsync(CancellationToken ct = default)
    {
        await ReloadCacheAsync(ct);
    }
    
    private async Task ReloadCacheAsync(CancellationToken ct)
    {
        _localizerCache.InvalidateCache();
        await _localizerCache.LoadAsync(ct);
    }

    private static TranslationResponse MapToResponse(Domain.Translation t)
    {
        return new TranslationResponse(
            t.Id, 
            t.Culture, 
            t.Key, 
            t.Value, 
            t.Description, 
            t.IsActive,
            t.IsSystemDefined,
            t.CreatedAtUtc, 
            t.CreatedBy, 
            t.ModifiedAtUtc, 
            t.ModifiedBy);
    }
}