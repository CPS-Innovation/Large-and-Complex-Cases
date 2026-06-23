using System.Net;

namespace CPS.ComplexCases.Common.Handlers;

public sealed class HttpResponseHandlerConfig
{
    public IReadOnlyDictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>> StatusCodeExceptionFactories { get; init; } =
        new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Exception>>();

    public required Func<HttpStatusCode, HttpRequestException, Exception> DefaultExceptionFactory { get; init; }
}
