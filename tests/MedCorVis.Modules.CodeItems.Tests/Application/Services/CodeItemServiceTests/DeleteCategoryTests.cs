namespace MedCorVis.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCorVis.Common.Results;
using MedCorVis.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class DeleteCategoryTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task DeleteCategoryAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.DeleteCategoryAsync(69);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_NotDeletable_ReturnsUnprocessableEntity()
    {
        var category = CreateCategory(isDeletable: false);

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_DELETABLE");
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_HasActiveItems_ReturnsUnprocessableEntity()
    {
        var category = CreateCategory();

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .CategoryHasActiveItemsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_HAS_ACTIVE_ITEMS");
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_ValidRequest_NoItems_SoftDeletesCategoryAndSaves()
    {
        var category = CreateCategory();

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .CategoryHasActiveItemsAsync(1, Arg.Any<CancellationToken>())
            .Returns(false);

        Repository
            .GetTrackedItemsByCategoryIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([]);

        Repository
            .GetTrackedTranslationsByEntityAsync(
                CodeItemTranslation.EntityTypeCategory,
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        await Repository
            .DidNotReceive()
            .GetTrackedTranslationsByEntityIdsAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyCollection<long>>(),
                Arg.Any<CancellationToken>());
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_ValidRequest_CascadesToItemsAndAllTranslations()
    {
        var category = CreateCategory();
        var item1 = CreateItem(categoryId: 1, code: "Consultation");
        var item2 = CreateItem(categoryId: 1, code: "FollowUp");

        var itemTranslation1 = CreateTranslation(CodeItemTranslation.EntityTypeItem, 1, "en");
        var itemTranslation2 = CreateTranslation(CodeItemTranslation.EntityTypeItem, 1, "fr");
        var categoryTranslation = CreateTranslation(CodeItemTranslation.EntityTypeCategory, 1, "en");

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .CategoryHasActiveItemsAsync(1, Arg.Any<CancellationToken>())
            .Returns(false);

        Repository
            .GetTrackedItemsByCategoryIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([item1, item2]);

        Repository
            .GetTrackedTranslationsByEntityIdsAsync(
                CodeItemTranslation.EntityTypeItem,
                Arg.Any<IReadOnlyCollection<long>>(),
                Arg.Any<CancellationToken>())
            .Returns([itemTranslation1, itemTranslation2]);

        Repository
            .GetTrackedTranslationsByEntityAsync(
                CodeItemTranslation.EntityTypeCategory,
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns([categoryTranslation]);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        item1.IsDeleted.Should().BeTrue();
        item2.IsDeleted.Should().BeTrue();
        itemTranslation1.IsDeleted.Should().BeTrue();
        itemTranslation2.IsDeleted.Should().BeTrue();
        categoryTranslation.IsDeleted.Should().BeTrue();
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_ValidRequest_NoCategoryTranslations_StillSucceeds()
    {
        var category = CreateCategory();
        var item = CreateItem(categoryId: 1);
        var itemTranslation = CreateTranslation(CodeItemTranslation.EntityTypeItem, 1, "en");

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .CategoryHasActiveItemsAsync(1, Arg.Any<CancellationToken>())
            .Returns(false);

        Repository
            .GetTrackedItemsByCategoryIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([item]);

        Repository
            .GetTrackedTranslationsByEntityIdsAsync(
                CodeItemTranslation.EntityTypeItem,
                Arg.Any<IReadOnlyCollection<long>>(),
                Arg.Any<CancellationToken>())
            .Returns([itemTranslation]);

        Repository
            .GetTrackedTranslationsByEntityAsync(
                CodeItemTranslation.EntityTypeCategory,
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        item.IsDeleted.Should().BeTrue();
        itemTranslation.IsDeleted.Should().BeTrue();
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}