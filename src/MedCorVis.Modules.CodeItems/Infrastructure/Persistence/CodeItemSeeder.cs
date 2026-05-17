namespace MedCorVis.Modules.CodeItems.Infrastructure.Persistence;

using MedCorVis.Common.Authorization;
using MedCorVis.Modules.CodeItems.Domain;
using MedCorVis.Modules.CodeItems.Infrastructure.Persistence.Logging;
using MedCorVis.Modules.CodeItems.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal sealed record CategorySeed(
    string Code,
    string? Description,
    int SortOrder,
    Dictionary<string, string> Labels,
    List<ItemSeed> Items);

internal sealed record ItemSeed(
    string Code,
    string? Description,
    int SortOrder,
    Dictionary<string, string> Labels);

internal static class CodeItemSeeder
{
    private static List<CategorySeed> AllSeeds =>
    [
        ..AppointmentSeeds.All,
        ..PatientSeeds.All,
        ..DoctorSeeds.All,
        ..MedicalRecordSeeds.All
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<CodeItemsDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(CodeItemSeeder));

        CodeItemSeederLogMessages.SeedingStarted(logger, null);

        var existingCategoryCodes = await db.Categories
            .AsNoTracking()
            .Select(c => c.Code)
            .ToHashSetAsync();

        var existingItemKeys = (await db.Items
            .AsNoTracking()
            .Select(i => new { i.CategoryId, i.Code })
            .ToListAsync())
            .Select(i => (i.CategoryId, i.Code))
            .ToHashSet();

        var existingTranslationKeys = (await db.Translations
            .AsNoTracking()
            .Select(t => new { t.EntityType, t.EntityId, t.Culture })
            .ToListAsync())
            .Select(t => (t.EntityType, t.EntityId, t.Culture))
            .ToHashSet();

        var categoriesSeeded   = 0;
        var itemsSeeded        = 0;
        var translationsSeeded = 0;

        foreach (var seed in AllSeeds)
        {
            Category category;

            if (existingCategoryCodes.Contains(seed.Code))
            {
                CodeItemSeederLogMessages.CategoryAlreadyExists(logger, seed.Code, null);
                category = await db.Categories.FirstAsync(c => c.Code == seed.Code);
            }
            else
            {
                category = Category.Create(
                    seed.Code,
                    seed.Description,
                    seed.SortOrder,
                    isSystemDefined: true,
                    isEditable: true,
                    isDeletable: false,
                    SystemActors.System);

                db.Categories.Add(category);
                await db.SaveChangesAsync(); // needed — populates category.Id
                existingCategoryCodes.Add(seed.Code);
                categoriesSeeded++;
            }

            foreach (var (culture, label) in seed.Labels)
            {
                if (existingTranslationKeys.Contains((CodeItemTranslation.EntityTypeCategory, category.Id, culture)))
                {
                    CodeItemSeederLogMessages.TranslationAlreadyExists(
                        logger, CodeItemTranslation.EntityTypeCategory, category.Id, culture, null);
                    continue;
                }

                var translation = CodeItemTranslation.Create(
                    CodeItemTranslation.EntityTypeCategory,
                    category.Id,
                    culture,
                    label,
                    description: null,
                    isSystemDefined: true,
                    SystemActors.System);

                db.Translations.Add(translation);
                existingTranslationKeys.Add((CodeItemTranslation.EntityTypeCategory, category.Id, culture));
                translationsSeeded++;
            }

            foreach (var itemSeed in seed.Items)
            {
                CodeItem item;

                if (existingItemKeys.Contains((category.Id, itemSeed.Code)))
                {
                    CodeItemSeederLogMessages.ItemAlreadyExists(logger, itemSeed.Code, seed.Code, null);
                    item = await db.Items.FirstAsync(i => i.CategoryId == category.Id && i.Code == itemSeed.Code);
                }
                else
                {
                    item = CodeItem.Create(
                        category.Id,
                        itemSeed.Code,
                        itemSeed.Description,
                        itemSeed.SortOrder,
                        validFrom: null,
                        validTo:   null,
                        isSystemDefined: true,
                        isEditable: true,
                        isDeletable: false,
                        SystemActors.System);

                    db.Items.Add(item);
                    await db.SaveChangesAsync(); // needed — populates item.Id
                    existingItemKeys.Add((category.Id, itemSeed.Code));
                    itemsSeeded++;
                }

                foreach (var (culture, label) in itemSeed.Labels)
                {
                    if (existingTranslationKeys.Contains((CodeItemTranslation.EntityTypeItem, item.Id, culture)))
                    {
                        CodeItemSeederLogMessages.TranslationAlreadyExists(
                            logger, CodeItemTranslation.EntityTypeItem, item.Id, culture, null);
                        continue;
                    }

                    var translation = CodeItemTranslation.Create(
                        CodeItemTranslation.EntityTypeItem,
                        item.Id,
                        culture,
                        label,
                        description: null,
                        isSystemDefined: true,
                        SystemActors.System);

                    db.Translations.Add(translation);
                    existingTranslationKeys.Add((CodeItemTranslation.EntityTypeItem, item.Id, culture));
                    translationsSeeded++;
                }
            }

            // Batch flush all translations for this category and its items in one call
            await db.SaveChangesAsync();
        }

        CodeItemSeederLogMessages.SeedingCompleted(logger, categoriesSeeded, itemsSeeded, translationsSeeded, null);
    }
}