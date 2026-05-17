namespace MedCorVis.Modules.CodeItems.Tests.Application.Services.CodeItemServiceTests;

using FluentAssertions;
using MedCorVis.Common.Localization;
using MedCorVis.Common.Results;
using MedCorVis.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class GetActiveItemsTests : CodeItemServiceTestBase
{
    [Fact]
    public async Task GetActiveItemsAsync_CategoryNotFound_ReturnsNotFound()
    {
        Repository
            .GetActiveByCategoryCodeAsync("nonexistent", 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(((Category?)null, (IReadOnlyList<CodeItem>)[]));

        var result = await Sut.GetActiveItemsAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error!.Code.Should().Be("CODEITEMS_CATEGORY_NOT_FOUND");
    }

    [Fact]
    public async Task GetActiveItemsAsync_NoItems_ReturnsEmptyEntries()
    {
        var category = CreateCategory("appointment.type");
    
        Repository
            .GetActiveByCategoryCodeAsync(
                "appointment.type", Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((category, (IReadOnlyList<CodeItem>)[]));
    
        var result = await Sut.GetActiveItemsAsync("appointment.type");
    
        result.IsSuccess.Should().BeTrue();
        result.Value!.CategoryCode.Should().Be("appointment.type");
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveItemsAsync_LabelFound_ReturnsLabelInResponse()
    {
        var category = CreateCategory("appointment.type");
        var item     = CreateItem(1, "Consultation");
    
        Repository
            .GetActiveByCategoryCodeAsync("appointment.type", 
                Arg.Any<DateOnly>(),Arg.Any<CancellationToken>())
            .Returns((category, (IReadOnlyList<CodeItem>)[item]));
    
        Repository
            .GetItemLabelsByCategoryAsync(category.Id, SupportedCultures.English, 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<long, string>)new Dictionary<long, string>
            {
                [item.Id] = "Consultation"
            });
    
        var result = await Sut.GetActiveItemsAsync("appointment.type");
    
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Code.Should().Be("Consultation");
        result.Value.Items[0].Label.Should().Be("Consultation");
    }

    [Fact]
    public async Task GetActiveItemsAsync_LabelNotFound_FallsBackToItemCode()
    {
        var category = CreateCategory("appointment.type");
        var item     = CreateItem(1, "Consultation");
    
        Repository
            .GetActiveByCategoryCodeAsync("appointment.type", 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((category, (IReadOnlyList<CodeItem>)[item]));
    
        Repository
            .GetItemLabelsByCategoryAsync(Arg.Any<long>(), Arg.Any<string>(), 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<long, string>)new Dictionary<long, string>());
    
        var result = await Sut.GetActiveItemsAsync("appointment.type");
    
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].Label.Should().Be("Consultation");
    }

    [Fact]
    public async Task GetActiveItemsAsync_LabelMissingForCulture_FallsBackToEnglish()
    {
        CurrentCultureService.Culture.Returns(SupportedCultures.French);
    
        var category = CreateCategory("appointment.type");
        var item     = CreateItem(1, "Consultation");
    
        Repository
            .GetActiveByCategoryCodeAsync("appointment.type", 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((category, (IReadOnlyList<CodeItem>)[item]));
    
        Repository
            .GetItemLabelsByCategoryAsync(category.Id, SupportedCultures.French, 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<long, string>)new Dictionary<long, string>());
    
        Repository
            .GetItemLabelsByCategoryAsync(category.Id, SupportedCultures.English, 
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<long, string>)new Dictionary<long, string>
            {
                [item.Id] = "Consultation"
            });
    
        var result = await Sut.GetActiveItemsAsync("appointment.type");
    
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].Label.Should().Be("Consultation");
    }
}