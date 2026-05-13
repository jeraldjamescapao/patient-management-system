namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class UpdateCategoryTests : CodeItemServiceTestBase
{
    private static readonly UpdateCategoryRequest ValidRequest =
        new(20, "Updated description");

    [Fact]
    public async Task UpdateCategoryAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.UpdateCategoryAsync(69, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpdateCategoryAsync_NotEditable_ReturnsUnprocessableEntity()
    {
        var category = CreateCategory(isEditable: false);

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        var result = await Sut.UpdateCategoryAsync(1, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_EDITABLE");
        await Repository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpdateCategoryAsync_ValidRequest_UpdatesAndSaves()
    {
        var category = CreateCategory();

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        var result = await Sut.UpdateCategoryAsync(1, ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Updated description");
        result.Value.SortOrder.Should().Be(20);
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}