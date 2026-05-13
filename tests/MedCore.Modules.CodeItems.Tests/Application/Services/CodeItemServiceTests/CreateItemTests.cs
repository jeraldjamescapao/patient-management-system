namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class CreateItemTests : CodeItemServiceTestBase
{
    private static readonly CreateItemRequest ValidRequest =
        new("CustomItem", 10, "A custom item");

    [Fact]
    public async Task CreateItemAsync_CategoryNotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.CreateItemAsync(69, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task CreateItemAsync_CodeAlreadyExists_ReturnsConflict()
    {
        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateCategory());

        Repository
            .ItemCodeExistsAsync(1, ValidRequest.Code, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await Sut.CreateItemAsync(1, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_CODE_EXISTS");
        await Repository
            .DidNotReceive()
            .AddItemAsync(Arg.Any<CodeItem>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task CreateItemAsync_ValidRequest_AddsAndSaves()
    {
        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateCategory());

        Repository
            .ItemCodeExistsAsync(1, ValidRequest.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await Sut.CreateItemAsync(1, ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("CustomItem");
        result.Value.IsSystemDefined.Should().BeFalse();
        result.Value.IsEditable.Should().BeTrue();
        result.Value.IsDeletable.Should().BeTrue();
        await Repository
            .Received(1)
            .AddItemAsync(Arg.Any<CodeItem>(), Arg.Any<CancellationToken>());
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}