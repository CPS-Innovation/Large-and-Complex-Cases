using System.Net.Http.Headers;
using CPS.ComplexCases.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.Common.Extensions;

public static class HttpRequestRequestHeadersExtensions
{
    public static Guid GetCorrelationId(this HttpRequestHeaders headers)
    {
        headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
        if (correlationIdValues == null)
        {
            throw new ArgumentException("Invalid correlationId. A valid GUID is required.", nameof(headers));
        }

        var correlationId = correlationIdValues.First();
        if (!Guid.TryParse(correlationId, out var currentCorrelationId) || currentCorrelationId == Guid.Empty)
        {
            throw new ArgumentException("Invalid correlationId. A valid GUID is required.", correlationId);
        }

        return currentCorrelationId;
    }

    public static Guid GetCorrelationId(this IHeaderDictionary headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var value))
        {
            throw new ArgumentException("Invalid correlationId. A valid GUID is required.", nameof(headers));
        }

        if (!Guid.TryParse(value[0], out var correlationId) || correlationId == Guid.Empty)
        {
            throw new ArgumentException("Invalid correlationId. A valid GUID is required.", value);
        }

        return correlationId;
    }
}