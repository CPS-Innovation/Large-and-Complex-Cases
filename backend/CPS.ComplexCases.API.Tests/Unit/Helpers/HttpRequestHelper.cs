using CPS.ComplexCases.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Text;
using System.Text.Json;

namespace CPS.ComplexCases.API.Tests.Unit.Helpers;

public static class HttpRequestStubHelper
{
    /// <summary>
    /// Creates a basic HTTP request with a correlation ID header
    /// </summary>
    public static HttpRequest CreateHttpRequest(Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates a basic HTTP request with a generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequest()
    {
        return CreateHttpRequest(Guid.NewGuid());
    }


    /// <summary>
    /// Creates an HTTP request with a JSON body from an object and specific correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestFor<T>(T obj, Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var jsonContent = JsonSerializer.Serialize(obj);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        request.ContentType = "application/json";
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates an HTTP request with a JSON body from an object and generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestFor<T>(T obj)
    {
        return CreateHttpRequestFor(obj, Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HTTP request with raw JSON content and specific correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithJsonBody(string jsonContent, Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        request.ContentType = "application/json";
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates an HTTP request with raw JSON content and generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithJsonBody(string jsonContent)
    {
        return CreateHttpRequestWithJsonBody(jsonContent, Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HTTP request with query parameters and specific correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithQueryParameters(Dictionary<string, string> queryParams, Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var queryCollection = new Dictionary<string, StringValues>();
        foreach (var param in queryParams)
        {
            queryCollection[param.Key] = new StringValues(param.Value);
        }

        request.Query = new QueryCollection(queryCollection);
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates a stub HttpRequest with optional query parameters, correlationId, and mockable cookies.
    /// </summary>
    public static HttpRequest CreateHttpRequestWithQueryParameters(
        Dictionary<string, string> queryParams,
        Guid correlationId,
        Mock<IResponseCookies>? cookiesMock = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        request.Scheme = "https";

        var queryCollection = new Dictionary<string, StringValues>();
        foreach (var param in queryParams)
        {
            queryCollection[param.Key] = new StringValues(param.Value);
        }
        request.Query = new QueryCollection(queryCollection);

        // Add correlationId header
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();

        // Inject mocked cookies if provided
        if (cookiesMock != null)
        {
            var cookiesFeature = new Mock<IResponseCookiesFeature>();
            cookiesFeature.Setup(f => f.Cookies).Returns(cookiesMock.Object);
            context.Features.Set(cookiesFeature.Object);
        }

        return request;
    }

    /// <summary>
    /// Creates an HTTP request with query parameters and generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithQueryParameters(Dictionary<string, string> queryParams)
    {
        return CreateHttpRequestWithQueryParameters(queryParams, Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HTTP request with a single query parameter and specific correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithQueryParameter(string key, string value, Guid correlationId)
    {
        var queryParams = new Dictionary<string, string> { [key] = value };
        return CreateHttpRequestWithQueryParameters(queryParams, correlationId);
    }

    /// <summary>
    /// Creates an HTTP request with a single query parameter and generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithQueryParameter(string key, string value)
    {
        return CreateHttpRequestWithQueryParameter(key, value, Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HTTP request with both JSON body and query parameters
    /// </summary>
    public static HttpRequest CreateHttpRequestWithJsonBodyAndQueryParameters<T>(
        T obj,
        Dictionary<string, string> queryParams,
        Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        // Set JSON body
        var jsonContent = JsonSerializer.Serialize(obj);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        request.ContentType = "application/json";

        // Set query parameters
        var queryCollection = new Dictionary<string, StringValues>();
        foreach (var param in queryParams)
        {
            queryCollection[param.Key] = new StringValues(param.Value);
        }
        request.Query = new QueryCollection(queryCollection);

        // Set correlation ID
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates an HTTP request with both JSON body and query parameters (generated correlation ID)
    /// </summary>
    public static HttpRequest CreateHttpRequestWithJsonBodyAndQueryParameters<T>(
        T obj,
        Dictionary<string, string> queryParams)
    {
        return CreateHttpRequestWithJsonBodyAndQueryParameters(obj, queryParams, Guid.NewGuid());
    }

    /// <summary>
    /// Creates an HTTP request with custom headers and specific correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithHeaders(
        Dictionary<string, string> headers,
        Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        // Set custom headers
        foreach (var header in headers)
        {
            request.Headers[header.Key] = header.Value;
        }

        // Always set correlation ID (override if provided in headers)
        request.Headers[HttpHeaderKeys.CorrelationId] = correlationId.ToString();
        return request;
    }

    /// <summary>
    /// Creates an HTTP request with custom headers and generated correlation ID
    /// </summary>
    public static HttpRequest CreateHttpRequestWithHeaders(Dictionary<string, string> headers)
    {
        return CreateHttpRequestWithHeaders(headers, Guid.NewGuid());
    }
}