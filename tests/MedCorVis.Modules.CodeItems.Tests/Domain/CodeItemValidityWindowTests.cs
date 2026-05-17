namespace MedCorVis.Modules.CodeItems.Tests.Domain;

using FluentAssertions;
using MedCorVis.Common.Exceptions;
using MedCorVis.Modules.CodeItems.Tests.Application.Services;
using MedCorVis.Modules.CodeItems.Domain;
using NSubstitute;
using Xunit;

public sealed class CodeItemValidityWindowTests : CodeItemServiceTestBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Create_ValidFromAfterValidTo_ThrowsDomainException()
    {
        var act = () => CodeItem.Create(
            1, 
            "Code", 
            null, 
            10,
            validFrom: Today.AddDays(5),
            validTo:   Today,
            isSystemDefined: false,
            isEditable: true,
            isDeletable: true,
            "system");

        act.Should().Throw<DomainException>()
            .WithMessage("*ValidFrom must be before ValidTo*");
    }
    
    [Fact]
    public void Create_ValidFromEqualsValidTo_ThrowsDomainException()
    {
        var act = () => CodeItem.Create(
            1, "Code", null, 10,
            validFrom: Today,
            validTo:   Today,
            isSystemDefined: false,
            isEditable: true,
            isDeletable: true,
            "system");

        act.Should().Throw<DomainException>();
    }
    
    [Fact]
    public void Create_NullValidFromAndValidTo_DoesNotThrow()
    {
        var act = () => CreateItem();

        act.Should().NotThrow();
    }
    
    [Fact]
    public void SetValidity_ValidFromAfterValidTo_ThrowsDomainException()
    {
        var item = CreateItem();

        var act = () => item.SetValidity(Today.AddDays(5), Today, "system");

        act.Should().Throw<DomainException>()
            .WithMessage("*ValidFrom must be before ValidTo*");
    }
    
    [Fact]
    public void SetValidity_NotEditable_ThrowsDomainException()
    {
        var item = CreateItem(isEditable: false);

        var act = () => item.SetValidity(Today, Today.AddDays(5), "system");

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be edited*");
    }
    
    [Fact]
    public void SetValidity_ValidWindow_SetsProperties()
    {
        var item = CreateItem();

        item.SetValidity(Today, Today.AddDays(30), "system");

        item.ValidFrom.Should().Be(Today);
        item.ValidTo.Should().Be(Today.AddDays(30));
    }
    
    [Fact]
    public void SetValidity_BothNull_ClearsWindow()
    {
        var item = CreateItem(validFrom: Today, validTo: Today.AddDays(30));

        item.SetValidity(null, null, "system");

        item.ValidFrom.Should().BeNull();
        item.ValidTo.Should().BeNull();
    }
}