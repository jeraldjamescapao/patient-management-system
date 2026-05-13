namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class GetItemsByCategoryTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task GetItemsByCategoryAsync_CategoryNotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.GetItemsByCategoryAsync(69);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task GetItemsByCategoryAsync_NoItems_ReturnsEmptyList()
    {
        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateCategory());

        Repository
            .GetItemsByCategoryIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut.GetItemsByCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetItemsByCategoryAsync_ItemsExist_ReturnsCorrectCount()
    {
        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateCategory());

        Repository
            .GetItemsByCategoryIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([CreateItem(1, "Consultation"), CreateItem(1, "FollowUp")]);

        var result = await Sut.GetItemsByCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }
}