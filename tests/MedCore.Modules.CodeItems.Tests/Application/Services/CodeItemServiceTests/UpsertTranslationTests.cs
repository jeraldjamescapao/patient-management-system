namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Authorization;
using MedCore.Common.Localization;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class UpsertTranslationTests : CodeItemServiceTestBase
{
    private static readonly UpsertTranslationRequest ValidRequest =
        new("Consultation", null);

    [Fact]
    public async Task UpsertCategoryTranslationAsync_UnsupportedCulture_ReturnsValidation()
    {
        var result = await Sut.UpsertCategoryTranslationAsync(1, "xx-XX", ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error!.Code.Should().Be("CODEITEMS_UNSUPPORTED_CULTURE");
    }
    
    [Fact]
    public async Task UpsertCategoryTranslationAsync_CategoryNotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.UpsertCategoryTranslationAsync(69, SupportedCultures.English, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpsertCategoryTranslationAsync_TranslationExists_UpdatesLabel()
    {
        var category = CreateCategory();
        var existing = CreateTranslation(
            CodeItemTranslation.EntityTypeCategory, 1, SupportedCultures.English, "Old Label");

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .GetTranslationAsync(
                CodeItemTranslation.EntityTypeCategory,
                1,
                SupportedCultures.English,
                Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await Sut.UpsertCategoryTranslationAsync(
            1, SupportedCultures.English, new UpsertTranslationRequest("New Label", null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Label.Should().Be("New Label");
        existing.Label.Should().Be("New Label");
        await Repository
            .DidNotReceive()
            .AddTranslationAsync(Arg.Any<CodeItemTranslation>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpsertCategoryTranslationAsync_TranslationNotExists_CreatesNew()
    {
        var category = CreateCategory();

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .GetTranslationAsync(
                CodeItemTranslation.EntityTypeCategory,
                1,
                SupportedCultures.French,
                Arg.Any<CancellationToken>())
            .Returns((CodeItemTranslation?)null);

        var result = await Sut.UpsertCategoryTranslationAsync(
            1, SupportedCultures.French, new UpsertTranslationRequest("Type de rendez-vous", null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Label.Should().Be("Type de rendez-vous");
        result.Value.Culture.Should().Be(SupportedCultures.French);
        await Repository
            .Received(1)
            .AddTranslationAsync(Arg.Any<CodeItemTranslation>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpsertItemTranslationAsync_UnsupportedCulture_ReturnsValidation()
    {
        var result = await Sut.UpsertItemTranslationAsync(1, "xx-XX", ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error!.Code.Should().Be("CODEITEMS_UNSUPPORTED_CULTURE");
    }
    
    [Fact]
    public async Task UpsertItemTranslationAsync_ItemNotFound_ReturnsNotFound()
    {
        Repository
            .GetItemByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((CodeItem?)null);

        var result = await Sut.UpsertItemTranslationAsync(69, SupportedCultures.English, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpsertItemTranslationAsync_DeactivatedTranslation_ReactivatesOnUpdate()
    {
        var item = CreateItem();
        var existing = CreateTranslation(
            CodeItemTranslation.EntityTypeItem, 1, SupportedCultures.English, "Old Label");

        existing.Deactivate(SystemActors.System);

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        Repository
            .GetTranslationAsync(
                CodeItemTranslation.EntityTypeItem,
                1,
                SupportedCultures.English,
                Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await Sut.UpsertItemTranslationAsync(
            1, SupportedCultures.English, new UpsertTranslationRequest("New Label", null));

        result.IsSuccess.Should().BeTrue();
        existing.IsActive.Should().BeTrue();
        existing.Label.Should().Be("New Label");
    }
}