namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class UpdateItemTests : CodeItemServiceTestBase
{
    private static readonly UpdateItemRequest ValidRequest =
        new(20, "Updated description");

    [Fact]
    public async Task UpdateItemAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetItemByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((CodeItem?)null);

        var result = await Sut.UpdateItemAsync(69, ValidRequest);

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

        var result = await Sut.UpdateItemAsync(1, ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.UnprocessableEntity);
        result.Error!.Code.Should().Be("CODEITEMS_ITEM_NOT_EDITABLE");
    }
    
    [Fact]
    public async Task UpdateItemAsync_ValidRequest_UpdatesAndSaves()
    {
        var item = CreateItem();

        Repository
            .GetItemByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(item);

        var result = await Sut.UpdateItemAsync(1, ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Updated description");
        result.Value.SortOrder.Should().Be(20);
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}