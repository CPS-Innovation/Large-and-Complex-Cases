using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.DDEI.Exceptions;
using CPS.ComplexCases.DDEI.Factories;

namespace CPS.ComplexCases.API.Integration.Tests.DDEI;

[Collection("Integration Tests")]
public class DdeiClientTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public DdeiClientTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task CanAuthenticate()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured - set DDEI__BaseUrl, DDEI__AccessKey, DDEI__Username, and DDEI__Password environment variables");

        // Act
        var response = await _fixture.DdeiClientTactical!.AuthenticateAsync(
            _fixture.Settings.DDEI.Username!,
            _fixture.Settings.DDEI.Password!);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.Token), "Authentication should return a valid token");
        Assert.False(string.IsNullOrEmpty(response.Cookies), "Authentication should return cookies");
    }

    [SkippableFact]
    public async Task CanGetAreas()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateBaseArg(cmsAuthValues, Guid.NewGuid());

        // Act
        var areas = await _fixture.DdeiClient!.GetAreasAsync(arg);

        // Assert
        Assert.NotNull(areas);
        Assert.NotNull(areas.AllAreas);
        Assert.NotEmpty(areas.AllAreas);
        Assert.True(areas.AllAreas.Count() > 0);
    }

    [SkippableFact]
    public async Task CanSearchCasesByUrn_WhenUrnProvided()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");
        Skip.If(string.IsNullOrEmpty(_fixture.Settings.DDEI.TestUrn), "DDEI__TestUrn not configured for case search test");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateUrnArg(cmsAuthValues, Guid.NewGuid(), _fixture.Settings.DDEI.TestUrn!);

        // Act
        var cases = await _fixture.DdeiClient!.ListCasesByUrnAsync(arg);

        // Assert
        Assert.NotNull(cases);
        Assert.True(cases.Count() > 0);
        Assert.Equal(_fixture.Settings.DDEI.TestUrn!, cases.First().Urn);
    }

    [SkippableFact]
    public async Task CanGetCase_WhenCaseIdProvided()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");
        Skip.If(!_fixture.Settings.DDEI.TestCaseId.HasValue, "DDEI__TestCaseId not configured for case retrieval test");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateCaseArg(cmsAuthValues, Guid.NewGuid(), _fixture.Settings.DDEI.TestCaseId!.Value);

        // Act
        var caseDetails = await _fixture.DdeiClient!.GetCaseAsync(arg);

        // Assert
        Assert.NotNull(caseDetails);
        Assert.Equal(_fixture.Settings.DDEI.TestCaseId.Value, caseDetails.CaseId);
    }

    [SkippableFact]
    public async Task CanGetCmsModernToken()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateBaseArg(cmsAuthValues, Guid.NewGuid());

        // Act
        var cmsModernToken = await _fixture.DdeiClient!.GetCmsModernTokenAsync(arg);

        // Assert
        Assert.False(string.IsNullOrEmpty(cmsModernToken), "CMS Modern Token should be returned");
    }

    [SkippableFact]
    public async Task Authenticate_WithInvalidCredentials_ThrowsException()
    {
        Skip.If(!_fixture.IsDdeiConfigured, "DDEI not configured");

        // Arrange
        var invalidUsername = "invalid-user@example.com";
        var invalidPassword = "invalid-password";

        // Act & Assert
        await Assert.ThrowsAsync<CmsUnauthorizedException>(
            async () => await _fixture.DdeiClientTactical!.AuthenticateAsync(invalidUsername, invalidPassword));
    }

    [SkippableFact]
    public async Task CanSearchCasesByOperationName_WhenOperationNameProvided()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");
        Skip.If(string.IsNullOrEmpty(_fixture.Settings.DDEI.TestOperationName), "DDEI__TestOperationName not configured for operation name search test");
        Skip.If(string.IsNullOrEmpty(_fixture.Settings.DDEI.TestCmsAreaCode), "DDEI__TestCmsAreaCode not configured for operation name search test");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateOperationNameArg(
            cmsAuthValues,
            Guid.NewGuid(),
            _fixture.Settings.DDEI.TestOperationName!,
            _fixture.Settings.DDEI.TestCmsAreaCode!);

        // Act
        var cases = await _fixture.DdeiClient!.ListCasesByOperationNameAsync(arg);

        // Assert
        Assert.NotNull(cases);
        Assert.True(cases.Count() > 0);
        Assert.Equal(_fixture.Settings.DDEI.TestOperationName!, cases.First().OperationName);
    }

    [SkippableFact]
    public async Task CanSearchCasesByDefendantName_WhenDefendantNameProvided()
    {
        Skip.If(!_fixture.IsDdeiAuthConfigured, "DDEI auth not configured");
        Skip.If(string.IsNullOrEmpty(_fixture.Settings.DDEI.TestDefendantLastName), "DDEI__TestDefendantLastName not configured for defendant name search test");
        Skip.If(string.IsNullOrEmpty(_fixture.Settings.DDEI.TestCmsAreaCode), "DDEI__TestCmsAreaCode not configured for defendant name search test");

        // Arrange
        var cmsAuthValues = await _fixture.GetCmsAuthValuesAsync();
        var argFactory = new DdeiArgFactory();
        var arg = argFactory.CreateDefendantArg(
            cmsAuthValues,
            Guid.NewGuid(),
            _fixture.Settings.DDEI.TestDefendantLastName!,
            _fixture.Settings.DDEI.TestCmsAreaCode!);

        // Act
        var cases = await _fixture.DdeiClient!.ListCasesByDefendantNameAsync(arg);

        // Assert
        Assert.NotNull(cases);
        Assert.True(cases.Count() > 0);
        Assert.Contains(_fixture.Settings.DDEI.TestDefendantLastName!, cases.First().LeadDefendantSurname);
    }
}
