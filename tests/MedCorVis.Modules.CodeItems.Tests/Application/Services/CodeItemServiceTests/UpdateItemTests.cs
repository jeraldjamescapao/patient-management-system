namespace MedCorVis.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCorVis.Common.Results;
using MedCorVis.Modules.CodeItems.Application.Contracts.Requests;
using MedCorVis.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class UpdateItemTests : CodeItemServiceTestBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    
    private static readonly UpdateItemRequest ValidRequest =
        new(20, "Updated description");

    [Fact]
    public async Task UpdateItemAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetItemByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((CodeItem?)null);

        var result = await Sut.UpdateItemAsync(1, 69, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_FOUND");
    }
    
    [Fact]
    public async Task UpdateItemAsync_NotEditable_ReturnsUnprocessableEntity()
    {
        var item = CreateItem(isEditable: false);

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var result = await Sut.UpdateItemAsync(1, 1, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_EDITABLE");
        await Repository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpdateItemAsync_ValidRequest_UpdatesAndSaves()
    {
        var item = CreateItem();

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var result = await Sut.UpdateItemAsync(1, 1, ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Updated description");
        result.Value.SortOrder.Should().Be(20);
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpdateItemAsync_ItemBelongsToDifferentCategory_ReturnsNotFound()
    {
        var item = CreateItem(categoryId: 99);

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var result = await Sut.UpdateItemAsync(categoryId: 1, id: 1, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_FOUND");
        await Repository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UpdateItemAsync_WithValidityWindow_SetsValidityOnItem()
    {
        var item = CreateItem();

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var request = new UpdateItemRequest(10, null, Today, Today.AddDays(30));

        var result = await Sut.UpdateItemAsync(1, 1, request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ValidFrom.Should().Be(Today);
        result.Value.ValidTo.Should().Be(Today.AddDays(30));
    }
    
    [Fact]
    public async Task UpdateItemAsync_ClearValidityWindow_SetsNulls()
    {
        var item = CreateItem(validFrom: Today, validTo: Today.AddDays(30));

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var request = new UpdateItemRequest(10, null, null, null);

        var result = await Sut.UpdateItemAsync(1, 1, request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ValidFrom.Should().BeNull();
        result.Value.ValidTo.Should().BeNull();
    }
}