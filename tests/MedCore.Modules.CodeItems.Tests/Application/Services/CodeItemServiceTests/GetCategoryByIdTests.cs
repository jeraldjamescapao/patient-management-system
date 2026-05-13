namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCore.Common.Results;
using MedCore.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class GetCategoryByIdTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task GetCategoryByIdAsync_NotFound_ReturnsNotFound()
    {
        Repository
            .GetCategoryByIdAsync(69, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var result = await Sut.GetCategoryByIdAsync(69);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }
    
    [Fact]
    public async Task GetCategoryByIdAsync_Exists_ReturnsCorrectShape()
    {
        var category = CreateCategory("appointment.type");

        Repository
            .GetCategoryByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        var result = await Sut.GetCategoryByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("appointment.type");
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsEditable.Should().BeTrue();
        result.Value.IsDeletable.Should().BeTrue();
    }
}