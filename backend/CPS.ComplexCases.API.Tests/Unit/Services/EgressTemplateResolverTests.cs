using CPS.ComplexCases.API.Domain.Configuration;
using CPS.ComplexCases.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class EgressTemplateResolverTests
{
    private const string DefaultTemplateId = "59a6855307087630eb190282";
    private const string CentralCaseworkDivisionsTemplateId = "53e7de1005b6a674dd9a30a3";
    private const string CentralCaseworkDivisionsAreaNames =
        "Counter Terrorism Division,SEOCID Int London and SE Div,SEOCID Regional and Wales Div,Organised Crime Division,Special Crime Division,Proceeds of Crime";

    private readonly Mock<IOptionsMonitor<EgressTemplateOptions>> _optionsMonitorMock;
    private readonly Mock<ILogger<EgressTemplateResolver>> _loggerMock;
    private readonly EgressTemplateResolver _resolver;
    private EgressTemplateOptions _options;

    public EgressTemplateResolverTests()
    {
        _options = CreateOptions();
        _optionsMonitorMock = new Mock<IOptionsMonitor<EgressTemplateOptions>>();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(() => _options);
        _loggerMock = new Mock<ILogger<EgressTemplateResolver>>();
        _resolver = new EgressTemplateResolver(_optionsMonitorMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("Counter Terrorism Division")]
    [InlineData("SEOCID Int London and SE Div")]
    [InlineData("SEOCID Regional and Wales Div")]
    [InlineData("Organised Crime Division")]
    [InlineData("Special Crime Division")]
    [InlineData("Proceeds of Crime")]
    public void ResolveTemplate_CentralCaseworkDivisionArea_ReturnsCentralCaseworkDivisionsTemplate(string areaName)
    {
        var result = _resolver.ResolveTemplate(areaName);

        Assert.Equal(CentralCaseworkDivisionsTemplateId, result.TemplateId);
        Assert.Equal("Central Casework Divisions", result.TemplateName);
        Assert.False(result.IsDefaultFallback);
    }

    [Theory]
    [InlineData("special crime division")]
    [InlineData("  Special Crime Division  ")]
    [InlineData("PROCEEDS OF CRIME")]
    public void ResolveTemplate_AreaNameDiffersByCaseOrWhitespace_ReturnsCentralCaseworkDivisionsTemplate(string areaName)
    {
        var result = _resolver.ResolveTemplate(areaName);

        Assert.Equal(CentralCaseworkDivisionsTemplateId, result.TemplateId);
        Assert.False(result.IsDefaultFallback);
    }

    [Theory]
    [InlineData("Wessex")]
    [InlineData("London North")]
    [InlineData("Special Crime")]
    public void ResolveTemplate_NonCentralArea_ReturnsDefaultTemplate(string areaName)
    {
        var result = _resolver.ResolveTemplate(areaName);

        Assert.Equal(DefaultTemplateId, result.TemplateId);
        Assert.Equal("CPS Areas", result.TemplateName);
        Assert.True(result.IsDefaultFallback);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveTemplate_UnassignedArea_ReturnsDefaultTemplate(string? areaName)
    {
        var result = _resolver.ResolveTemplate(areaName);

        Assert.Equal(DefaultTemplateId, result.TemplateId);
        Assert.True(result.IsDefaultFallback);
    }

    [Fact]
    public void ResolveTemplate_EmptyAreaNameList_ReturnsDefaultTemplate()
    {
        _options = CreateOptions(areaNames: string.Empty);

        var result = _resolver.ResolveTemplate("Special Crime Division");

        Assert.Equal(DefaultTemplateId, result.TemplateId);
        Assert.True(result.IsDefaultFallback);
    }

    [Fact]
    public void ResolveTemplate_CentralTemplateIdNotConfigured_ReturnsDefaultTemplate()
    {
        _options = CreateOptions(centralTemplateId: string.Empty);

        var result = _resolver.ResolveTemplate("Special Crime Division");

        Assert.Equal(DefaultTemplateId, result.TemplateId);
        Assert.True(result.IsDefaultFallback);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveTemplate_DefaultTemplateIdNotConfigured_Throws(string defaultTemplateId)
    {
        _options = CreateOptions(defaultTemplateId: defaultTemplateId);

        Assert.Throws<InvalidOperationException>(() => _resolver.ResolveTemplate("Wessex"));
    }

    [Fact]
    public void ResolveTemplate_ConfigurationChanges_UsesUpdatedMappings()
    {
        Assert.True(_resolver.ResolveTemplate("Wessex").IsDefaultFallback);

        _options = CreateOptions(
            centralTemplateId: "updated-ccd-template",
            areaNames: $"{CentralCaseworkDivisionsAreaNames},Wessex");

        var result = _resolver.ResolveTemplate("Wessex");

        Assert.Equal("updated-ccd-template", result.TemplateId);
        Assert.False(result.IsDefaultFallback);
    }

    private static EgressTemplateOptions CreateOptions(
        string defaultTemplateId = DefaultTemplateId,
        string centralTemplateId = CentralCaseworkDivisionsTemplateId,
        string areaNames = CentralCaseworkDivisionsAreaNames) => new()
        {
            DefaultTemplateId = defaultTemplateId,
            CentralCaseworkDivisionsTemplateId = centralTemplateId,
            CentralCaseworkDivisionsAreaNames = areaNames
        };
}
