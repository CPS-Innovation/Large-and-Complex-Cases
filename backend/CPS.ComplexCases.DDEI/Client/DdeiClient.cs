using System.Net;
using System.Text.Json;
using CPS.ComplexCases.DDEI.Exceptions;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;
using CPS.ComplexCases.DDEI.Tactical.Client;
using CPS.ComplexCases.DDEI.Tactical.Factories;
using CPS.ComplexCases.DDEI.Tactical.Mappers;
using CPS.ComplexCases.DDEI.Tactical.Models.Dto;
using Microsoft.Extensions.Logging;
using TacticalDomain = CPS.ComplexCases.DDEI.Tactical.Models.Response;

namespace CPS.ComplexCases.DDEI.Client;

public class DdeiClient(ILogger<DdeiClient> logger,
 HttpClient httpClient,
 IDdeiRequestFactory ddeiRequestFactory,
 IDdeiArgFactory ddeiArgFactory,
 ICaseDetailsMapper caseDetailsMapper,
 IAreasMapper areasMapper,
 IDdeiRequestFactoryTactical ddeiRequestFactoryTactical,
 IAuthenticationResponseMapper authenticationResponseMapper) : IDdeiClient, IDdeiClientTactical
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<DdeiClient> _logger = logger;
  private readonly IDdeiRequestFactory _ddeiRequestFactory = ddeiRequestFactory;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;
  private readonly ICaseDetailsMapper _caseDetailsMapper = caseDetailsMapper;
  private readonly IAreasMapper _areasMapper = areasMapper;
  private readonly IDdeiRequestFactoryTactical _ddeiRequestFactoryTactical = ddeiRequestFactoryTactical;
  private readonly IAuthenticationResponseMapper _authenticationResponseMapper = authenticationResponseMapper;

  public async Task<TacticalDomain.AuthenticationResponse> AuthenticateAsync(string username, string password)
  {
    var response = await CallDdei<AuthenticationResponse>(_ddeiRequestFactoryTactical.CreateAuthenticateRequest(username, password));
    return _authenticationResponseMapper.Map(response);
  }

  public async Task<IEnumerable<CaseDto>> ListCasesByUrnAsync(DdeiUrnArgDto arg)
  {
    var caseIdentifiers = await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiRequestFactory.CreateListCasesByUrnRequest(arg));

    var calls = caseIdentifiers.Select(async caseIdentifier =>
         await GetCaseInternalAsync(_ddeiArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

    var cases = await Task.WhenAll(calls);
    return cases.Select(_caseDetailsMapper.MapCaseDetails);
  }

  public async Task<IEnumerable<CaseDto>> ListCasesByOperationNameAsync(DdeiOperationNameArgDto arg)
  {
    var caseIdentifiers = await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiRequestFactory.CreateListCasesByOperationNameRequest(arg));

    var calls = caseIdentifiers.Select(async caseIdentifier =>
     await GetCaseInternalAsync(_ddeiArgFactory.CreateCaseArgFromOperationNameArg(arg, caseIdentifier.Id)));

    var cases = await Task.WhenAll(calls);
    return cases.Select(_caseDetailsMapper.MapCaseDetails);
  }

  public async Task<IEnumerable<CaseDto>> ListCasesByDefendantNameAsync(DdeiDefendantNameArgDto arg)
  {
    var caseIdentifiers = await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiRequestFactory.CreateListCasesByDefendantRequest(arg));

    var calls = caseIdentifiers.Select(async caseIdentifier =>
     await GetCaseInternalAsync(_ddeiArgFactory.CreateCaseArgFromDefendantArg(arg, caseIdentifier.Id)));

    var cases = await Task.WhenAll(calls);
    return cases.Select(_caseDetailsMapper.MapCaseDetails);
  }

  public async Task<AreasDto> GetAreasAsync(DdeiBaseArgDto arg)
  {
    var userFilteredDataTask = CallDdei<DdeiUserFilteredDataDto>(_ddeiRequestFactory.CreateUserFilteredDataRequest(arg));
    var allUnitsTask = CallDdei<IEnumerable<DdeiUnitDto>>(_ddeiRequestFactory.CreateListUnitsRequest(arg));
    var userDataTask = CallDdei<DdeiUserDataDto>(_ddeiRequestFactory.CreateUserDataRequest(arg));

    await Task.WhenAll(userFilteredDataTask, allUnitsTask, userDataTask);

    var userFilteredData = await userFilteredDataTask;
    var allUnits = await allUnitsTask;
    var userData = await userDataTask;

    return _areasMapper.MapAreas(userFilteredData, userData, allUnits);
  }

  public async Task<CaseDto> GetCaseAsync(DdeiCaseIdArgDto arg)
  {
    var caseSummary = await GetCaseInternalAsync(arg);
    return _caseDetailsMapper.MapCaseDetails(caseSummary);
  }

  public async Task<string?> GetCmsModernTokenAsync(DdeiBaseArgDto arg)
  {
    var response = await CallDdei<DdeiCmsModernTokenDto>(_ddeiRequestFactory.CreateGetCmsModernTokenRequest(arg));
    return response.CmsModernToken;
  }

  private async Task<DdeiCaseSummaryDto> GetCaseInternalAsync(DdeiCaseIdArgDto arg) =>
      await CallDdei<DdeiCaseSummaryDto>(_ddeiRequestFactory.CreateGetCaseRequest(arg));

  private async Task<T> CallDdei<T>(HttpRequestMessage request)
  {
    using var response = await CallDdei(request);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<T>(content) ?? throw new InvalidOperationException("Deserialization returned null.");
    return result;
  }

  private async Task<HttpResponseMessage> CallDdei(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
  {
    var response = await _httpClient.SendAsync(request);
    try
    {
      if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
      {
        return response;
      }

      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        throw new CmsUnauthorizedException();
      }

      var content = await response.Content.ReadAsStringAsync();
      throw new HttpRequestException(content);
    }
    catch (HttpRequestException exception)
    {
      throw new DdeiClientException(response.StatusCode, exception);
    }
  }
}