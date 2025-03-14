using System.Net;
using System.Text.Json;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.DDEI.Client;

public class DdeiClient(ILogger<DdeiClient> logger,
 HttpClient httpClient,
 IDdeiRequestFactory ddeiRequestFactory,
 IDdeiArgFactory ddeiArgFactory,
 ICaseDetailsMapper caseDetailsMapper,
 IUserDetailsMapper userDetailsMapper) : IDdeiClient
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<DdeiClient> _logger = logger;
  private readonly IDdeiRequestFactory _ddeiRequestFactory = ddeiRequestFactory;
  private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;
  private readonly ICaseDetailsMapper _caseDetailsMapper = caseDetailsMapper;
  private readonly IUserDetailsMapper _userDetailsMapper = userDetailsMapper;

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

  public async Task<IEnumerable<AreaDto>> GetUserCmsAreasAsync(DdeiBaseArgDto arg)
  {
    var userFilteredData = await CallDdei<DdeiUserFilteredDataDto>(_ddeiRequestFactory.CreateUserFilteredDataRequest(arg));

    return _userDetailsMapper.MapUserAreas(userFilteredData);
  }

  private async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdArgDto arg) =>
      await CallDdei<DdeiCaseDetailsDto>(_ddeiRequestFactory.CreateGetCaseRequest(arg));

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

      var content = await response.Content.ReadAsStringAsync();
      throw new HttpRequestException(content);
    }
    catch (HttpRequestException exception)
    {
      _logger.LogError(exception, "Error sending request to DDEI service");
      throw;
    }
  }
}