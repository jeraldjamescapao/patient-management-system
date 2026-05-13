namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Domain;
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
    public async Task DeleteCategoryAsync_ValidRequest_SoftDeletesAndSaves()
    {
        var category = CreateCategory();

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        Repository
            .CategoryHasActiveItemsAsync(1, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await Sut.DeleteCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}