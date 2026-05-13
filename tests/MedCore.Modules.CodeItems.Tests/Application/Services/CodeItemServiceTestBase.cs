using System.Security.Cryptography.X509Certificates;

namespace MedCore.Modules.CodeItems.Tests.Application.Services;

using MedCore.Common.Authorization;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Modules.CodeItems.Application.Abstractions;
using MedCore.Modules.CodeItems.Application.Services;
using MedCore.Modules.CodeItems.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

public abstract class CodeItemServiceTestBase
{
    internal readonly ICodeItemRepository Repository;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly ICurrentCultureService CurrentCultureService;
    protected readonly ICodeItemService Sut;
    
    protected CodeItemServiceTestBase()
    {
        Repository             = Substitute.For<ICodeItemRepository>();
        CurrentUserService     = Substitute.For<ICurrentUserService>();
        CurrentCultureService  = Substitute.For<ICurrentCultureService>();

        CurrentUserService.UserId.Returns(SystemActors.System);
        CurrentCultureService.Culture.Returns(SupportedCultures.English);

        Sut = new CodeItemService(
            Repository,
            CurrentUserService,
            CurrentCultureService,
            NullLogger<CodeItemService>.Instance);
    }
    
    internal static Category CreateCategory(
        string code         = "appointment.type",
        bool isActive       = true,
        bool isSystemDefined = false,
        bool isEditable     = true,
        bool isDeletable    = true)
    {
        var category = Category.Create(
            code,
            "Test category description",
            10,
            isSystemDefined,
            isEditable,
            isDeletable,
            SystemActors.System);

        if (!isActive)
            category.Deactivate(SystemActors.System);

        return category;
    }
    
    internal static CodeItem CreateItem(
        long categoryId     = 1,
        string code         = "Consultation",
        bool isActive       = true,
        bool isSystemDefined = false,
        bool isEditable     = true,
        bool isDeletable    = true)
    {
        var item = CodeItem.Create(
            categoryId,
            code,
            "Test item description",
            10,
            isSystemDefined,
            isEditable,
            isDeletable,
            SystemActors.System);
        
        if (!isActive)
            item.Deactivate(SystemActors.System);

        return item;
    }
    
    internal static CodeItemTranslation CreateTranslation(
        string entityType   = CodeItemTranslation.EntityTypeItem,
        long entityId       = 1,
        string culture      = SupportedCultures.English,
        string label        = "Consultation")
    {
        return CodeItemTranslation.Create(
            entityType,
            entityId,
            culture,
            label,
            null,
            false,
            SystemActors.System);
    }
}