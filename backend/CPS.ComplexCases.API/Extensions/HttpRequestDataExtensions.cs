using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;

namespace CPS.ComplexCases.API.Extensions;

public static class HttpRequestDataExtentions
{
  public static Guid EstablishCorrelation(this HttpRequestData req)
  {
    if (req.Headers is null)
    {
      throw new ArgumentNullException(nameof(HttpRequestData.Headers));
    }

    if (req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIds) &&
        correlationIds.Any() &&
        Guid.TryParse(correlationIds.First(), out var parsedCorrelationId) &&
        parsedCorrelationId != Guid.Empty)
    {
      return parsedCorrelationId;
    }
    else
    {
      throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(HttpRequestData.Headers));
    }
  }
}