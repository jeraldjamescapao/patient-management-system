namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class DeleteItemTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task DeleteItemAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetItemByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((CodeItem?)null);

        var result = await Sut.DeleteItemAsync(69);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_FOUND");
    }
    
    [Fact]
    public async Task DeleteItemAsync_NotDeletable_ReturnsUnprocessableEntity()
    {
        var item = CreateItem(isDeletable: false);

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var result = await Sut.DeleteItemAsync(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_DELETABLE");
    }
    
    [Fact]
    public async Task DeleteItemAsync_ValidRequest_CascadesTranslationsAndSaves()
    {
        var item = CreateItem();

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var translation1 = CreateTranslation(entityId: 1, culture: "en");
        var translation2 = CreateTranslation(entityId: 2, culture: "fr");
        var translation3 = CreateTranslation(entityId: 3, culture: "de");
        
        Repository
            .GetTranslationsByEntityAsync(
                CodeItemTranslation.EntityTypeItem,
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns([ translation1, translation2, translation3]);

        var result = await Sut.DeleteItemAsync(1);

        result.IsSuccess.Should().BeTrue();
        item.IsDeleted.Should().BeTrue();
        translation1.IsDeleted.Should().BeTrue();
        translation2.IsDeleted.Should().BeTrue();
        translation3.IsDeleted.Should().BeTrue();
        await Repository
            .Received(1)
            .GetTranslationsByEntityAsync(
                CodeItemTranslation.EntityTypeItem,
                Arg.Any<long>(),
                Arg.Any<CancellationToken>());
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}