using System.Net;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace CPS.ComplexCases.DDEI.Tests.Client;

public class DdeiClientTests
{
  private readonly Fixture _fixture;
  private readonly Mock<ILogger<DdeiClient>> _loggerMock;
  private readonly Mock<IDdeiRequestFactory> _ddeiRequestFactoryMock;
  private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
  private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
  private readonly Mock<ICaseDetailsMapper> _caseDetailsMapperMock;
  private readonly HttpClient _httpClient;
  private readonly DdeiClient _client;
  private readonly DdeiUrnArgDto _ddeiCaseIdentifiersArgDto;
  private readonly DdeiOperationNameArgDto _ddeiOperationNameArgDto;
  private readonly DdeiDefendantNameArgDto _ddeiDefendantNameArgDto;
  private const string TestUrl = "https://example.com";

  public DdeiClientTests()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization());

    _loggerMock = _fixture.Freeze<Mock<ILogger<DdeiClient>>>();
    _ddeiRequestFactoryMock = new Mock<IDdeiRequestFactory>();
    _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
    _caseDetailsMapperMock = new Mock<ICaseDetailsMapper>();
    _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

    _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
    {
      BaseAddress = new Uri(TestUrl)
    };

    _ddeiCaseIdentifiersArgDto = _fixture.Create<DdeiUrnArgDto>();
    _ddeiOperationNameArgDto = _fixture.Create<DdeiOperationNameArgDto>();
    _ddeiDefendantNameArgDto = _fixture.Create<DdeiDefendantNameArgDto>();

    _client = new DdeiClient(
      _loggerMock.Object,
      _httpClient,
      _ddeiRequestFactoryMock.Object,
      _ddeiArgFactoryMock.Object,
      _caseDetailsMapperMock.Object
    );
  }

  [Fact]
  public async Task ListCasesByUrnAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
  {
    var mockRequest = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{_fixture.Create<string>()}/cases");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByUrnRequest(_ddeiCaseIdentifiersArgDto))
        .Returns(mockRequest);

    var urnListResponse = _fixture.Create<IEnumerable<DdeiCaseIdentifiersDto>>();
    SetupHttpMockResponses(
        ("urn", urnListResponse, HttpStatusCode.BadRequest)
    );
    await Assert.ThrowsAsync<HttpRequestException>(() => _client.ListCasesByUrnAsync(_ddeiCaseIdentifiersArgDto));
  }

  [Fact]
  public async Task ListCasesByUrnAsync_ReturnsMappedCases_WhenResponseIsSuccessful()
  {
    // Arrange
    var mockUrnRequest = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{_fixture.Create<string>()}/cases");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByUrnRequest(_ddeiCaseIdentifiersArgDto))
        .Returns(mockUrnRequest);

    var caseIdentifiers = _fixture.CreateMany<DdeiCaseIdentifiersDto>(3).ToList();
    var mockCaseRequests = new List<HttpRequestMessage>();
    var mockCaseArgs = new List<DdeiCaseIdArgDto>();
    var caseDetailsDtos = new List<DdeiCaseDetailsDto>();
    var expectedCaseDtos = new List<CaseDto>();

    var responseSetup = new List<(string type, object response, HttpStatusCode statusCode)>
        {
            ("urnList", caseIdentifiers, HttpStatusCode.OK)
        };

    for (int i = 0; i < caseIdentifiers.Count; i++)
    {
      var caseArg = _fixture.Create<DdeiCaseIdArgDto>();
      mockCaseArgs.Add(caseArg);

      _ddeiArgFactoryMock
          .Setup(f => f.CreateCaseArgFromUrnArg(_ddeiCaseIdentifiersArgDto, caseIdentifiers[i].Id))
          .Returns(caseArg);

      var caseRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{caseIdentifiers[i].Id}");
      mockCaseRequests.Add(caseRequest);

      _ddeiRequestFactoryMock
          .Setup(f => f.CreateGetCaseRequest(caseArg))
          .Returns(caseRequest);

      var caseDetailsDto = _fixture.Create<DdeiCaseDetailsDto>();
      caseDetailsDtos.Add(caseDetailsDto);

      var caseDto = _fixture.Create<CaseDto>();
      expectedCaseDtos.Add(caseDto);

      _caseDetailsMapperMock
          .Setup(m => m.MapCaseDetails(It.Is<DdeiCaseDetailsDto>(dto =>
              dto.Summary.Id == caseDetailsDto.Summary.Id)))
          .Returns(caseDto);

      responseSetup.Add(("caseDetails", caseDetailsDto, HttpStatusCode.OK));
    }

    SetupHttpMockResponses(responseSetup.ToArray());

    // Act
    var result = await _client.ListCasesByUrnAsync(_ddeiCaseIdentifiersArgDto);

    // Assert
    var resultList = result.ToList();
    Assert.Equal(expectedCaseDtos.Count, resultList.Count);
    for (int i = 0; i < expectedCaseDtos.Count; i++)
    {
      Assert.Same(expectedCaseDtos[i], resultList[i]);
    }
  }

  [Fact]
  public async Task ListCasesByOperationNameAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
  {
    // Arrange
    var mockRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/search?operationName={_fixture.Create<string>()}&cmsAreaCode={_fixture.Create<string>()}");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByOperationNameRequest(_ddeiOperationNameArgDto))
        .Returns(mockRequest);

    var operationNameList = _fixture.Create<IEnumerable<DdeiCaseIdentifiersDto>>();
    SetupHttpMockResponses(
        ("operationName", operationNameList, HttpStatusCode.BadRequest)
    );

    // Act & Assert
    await Assert.ThrowsAsync<HttpRequestException>(() => _client.ListCasesByOperationNameAsync(_ddeiOperationNameArgDto));
  }

  [Fact]
  public async Task ListCasesByOperationNameAsync_ReturnsMappedCases_WhenResponseIsSuccessful()
  {
    // Arrange
    var mockUrnRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/search?operationName={_fixture.Create<string>()}&cmsAreaCode={_fixture.Create<string>()}");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByOperationNameRequest(_ddeiOperationNameArgDto))
        .Returns(mockUrnRequest);

    var caseIdentifiers = _fixture.CreateMany<DdeiCaseIdentifiersDto>(3).ToList();
    var mockCaseRequests = new List<HttpRequestMessage>();
    var mockCaseArgs = new List<DdeiCaseIdArgDto>();
    var caseDetailsDtos = new List<DdeiCaseDetailsDto>();
    var expectedCaseDtos = new List<CaseDto>();

    var responseSetup = new List<(string type, object response, HttpStatusCode statusCode)>
        {
            ("operationList", caseIdentifiers, HttpStatusCode.OK)
        };

    for (int i = 0; i < caseIdentifiers.Count; i++)
    {
      var caseArg = _fixture.Create<DdeiCaseIdArgDto>();
      mockCaseArgs.Add(caseArg);

      _ddeiArgFactoryMock
          .Setup(f => f.CreateCaseArgFromOperationNameArg(_ddeiOperationNameArgDto, caseIdentifiers[i].Id))
          .Returns(caseArg);

      var caseRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{caseIdentifiers[i].Id}");
      mockCaseRequests.Add(caseRequest);

      _ddeiRequestFactoryMock
          .Setup(f => f.CreateGetCaseRequest(caseArg))
          .Returns(caseRequest);

      var caseDetailsDto = _fixture.Create<DdeiCaseDetailsDto>();
      caseDetailsDtos.Add(caseDetailsDto);

      var caseDto = _fixture.Create<CaseDto>();
      expectedCaseDtos.Add(caseDto);

      _caseDetailsMapperMock
          .Setup(m => m.MapCaseDetails(It.Is<DdeiCaseDetailsDto>(dto =>
              dto.Summary.Id == caseDetailsDto.Summary.Id)))
          .Returns(caseDto);

      responseSetup.Add(("caseDetails", caseDetailsDto, HttpStatusCode.OK));
    }

    SetupHttpMockResponses(responseSetup.ToArray());

    // Act
    var result = await _client.ListCasesByOperationNameAsync(_ddeiOperationNameArgDto);

    // Assert
    var resultList = result.ToList();
    Assert.Equal(expectedCaseDtos.Count, resultList.Count);
    for (int i = 0; i < expectedCaseDtos.Count; i++)
    {
      Assert.Same(expectedCaseDtos[i], resultList[i]);
    }
  }

  [Fact]
  public async Task ListCasesByDefendantNameAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
  {
    // Arrange
    var mockRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/search?defendantLastName={_fixture.Create<string>()}&cmsAreaCode={_fixture.Create<string>()}");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByDefendantRequest(_ddeiDefendantNameArgDto))
        .Returns(mockRequest);

    var defendantNameList = _fixture.Create<IEnumerable<DdeiCaseIdentifiersDto>>();
    SetupHttpMockResponses(
        ("defendantName", defendantNameList, HttpStatusCode.BadRequest)
    );

    // Act & Assert
    await Assert.ThrowsAsync<HttpRequestException>(() => _client.ListCasesByDefendantNameAsync(_ddeiDefendantNameArgDto));
  }

  [Fact]
  public async Task ListCasesByDefendantAsync_ReturnsMappedCases_WhenResponseIsSuccessful()
  {
    // Arrange
    var mockUrnRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/search?defendantLastName={_fixture.Create<string>()}&cmsAreaCode={_fixture.Create<string>()}");
    _ddeiRequestFactoryMock
        .Setup(f => f.CreateListCasesByDefendantRequest(_ddeiDefendantNameArgDto))
        .Returns(mockUrnRequest);

    var caseIdentifiers = _fixture.CreateMany<DdeiCaseIdentifiersDto>(3).ToList();
    var mockCaseRequests = new List<HttpRequestMessage>();
    var mockCaseArgs = new List<DdeiCaseIdArgDto>();
    var caseDetailsDtos = new List<DdeiCaseDetailsDto>();
    var expectedCaseDtos = new List<CaseDto>();

    var responseSetup = new List<(string type, object response, HttpStatusCode statusCode)>
        {
            ("defendantList", caseIdentifiers, HttpStatusCode.OK)
        };

    for (int i = 0; i < caseIdentifiers.Count; i++)
    {
      var caseArg = _fixture.Create<DdeiCaseIdArgDto>();
      mockCaseArgs.Add(caseArg);

      _ddeiArgFactoryMock
          .Setup(f => f.CreateCaseArgFromDefendantArg(_ddeiDefendantNameArgDto, caseIdentifiers[i].Id))
          .Returns(caseArg);

      var caseRequest = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{caseIdentifiers[i].Id}");
      mockCaseRequests.Add(caseRequest);

      _ddeiRequestFactoryMock
          .Setup(f => f.CreateGetCaseRequest(caseArg))
          .Returns(caseRequest);

      var caseDetailsDto = _fixture.Create<DdeiCaseDetailsDto>();
      caseDetailsDtos.Add(caseDetailsDto);

      var caseDto = _fixture.Create<CaseDto>();
      expectedCaseDtos.Add(caseDto);

      _caseDetailsMapperMock
          .Setup(m => m.MapCaseDetails(It.Is<DdeiCaseDetailsDto>(dto =>
              dto.Summary.Id == caseDetailsDto.Summary.Id)))
          .Returns(caseDto);

      responseSetup.Add(("caseDetails", caseDetailsDto, HttpStatusCode.OK));
    }

    SetupHttpMockResponses(responseSetup.ToArray());

    // Act
    var result = await _client.ListCasesByDefendantNameAsync(_ddeiDefendantNameArgDto);

    // Assert
    var resultList = result.ToList();
    Assert.Equal(expectedCaseDtos.Count, resultList.Count);
    for (int i = 0; i < expectedCaseDtos.Count; i++)
    {
      Assert.Same(expectedCaseDtos[i], resultList[i]);
    }
  }

  private void SetupHttpMockResponses(params (string type, object response, HttpStatusCode statusCode)[] responses)
  {
    var sequence = _httpMessageHandlerMock
        .Protected()
        .SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );

    foreach (var (_, response, statusCode) in responses)
    {
      var content = JsonSerializer.Serialize(response);
      sequence = sequence.ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content = new StringContent(content)
      });
    }
  }
}