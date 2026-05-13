namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Application.Contracts.Requests;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class CreateCategoryTests : CodeItemServiceTestBase
{
    private static readonly CreateCategoryRequest ValidRequest =
        new("custom.category", 10, "A custom category");

    [Fact]
    public async Task CreateCategoryAsync_CodeAlreadyExists_ReturnsConflict()
    {
        Repository
            .CategoryCodeExistsAsync(ValidRequest.Code, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await Sut.CreateCategoryAsync(ValidRequest);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_CODE_EXISTS");
        await Repository
            .DidNotReceive()
            .AddCategoryAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task CreateCategoryAsync_ValidRequest_AddsAndSaves()
    {
        Repository
            .CategoryCodeExistsAsync(ValidRequest.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await Sut.CreateCategoryAsync(ValidRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("custom.category");
        result.Value.IsSystemDefined.Should().BeFalse();
        result.Value.IsEditable.Should().BeTrue();
        result.Value.IsDeletable.Should().BeTrue();
        await Repository
            .Received(1)
            .AddCategoryAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await Repository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}