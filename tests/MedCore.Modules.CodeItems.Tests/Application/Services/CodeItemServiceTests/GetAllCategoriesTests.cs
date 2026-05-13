namespace MedCore.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using NSubstitute;
using Xunit;

public sealed class GetAllCategoriesTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task GetAllCategoriesAsync_NoneExist_ReturnsEmptyList()
    {
        Repository
            .GetAllCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await Sut.GetAllCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllCategoriesAsync_CategoriesExist_ReturnsCorrectCount()
    {
        Repository
            .GetAllCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns([CreateCategory("appointment.type"), CreateCategory("patient.type")]);

        var result = await Sut.GetAllCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsCorrectShape()
    {
        Repository
            .GetAllCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns([CreateCategory("appointment.type")]);

        var result = await Sut.GetAllCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value![0].Code.Should().Be("appointment.type");
        result.Value![0].IsActive.Should().BeTrue();
    }
}