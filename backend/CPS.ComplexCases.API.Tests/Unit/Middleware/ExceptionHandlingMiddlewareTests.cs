using System.Net;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Middleware;
using CPS.ComplexCases.DDEI.Exceptions;
using Polly.CircuitBreaker;

namespace CPS.ComplexCases.API.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
  [Fact]
  public void MapExceptionToStatusCode_BrokenCircuitException_MapsToServiceUnavailable()
  {
    var statusCode = ExceptionHandlingMiddleware.MapExceptionToStatusCode(new BrokenCircuitException());

    Assert.Equal(HttpStatusCode.ServiceUnavailable, statusCode);
  }

  [Fact]
  public void MapExceptionToStatusCode_BrokenCircuitExceptionWithMessage_MapsToServiceUnavailable()
  {
    var statusCode = ExceptionHandlingMiddleware.MapExceptionToStatusCode(
        new BrokenCircuitException("Circuit open"));

    Assert.Equal(HttpStatusCode.ServiceUnavailable, statusCode);
  }

  [Fact]
  public void MapExceptionToStatusCode_BadRequestException_MapsToBadRequest()
  {
    var statusCode = ExceptionHandlingMiddleware.MapExceptionToStatusCode(
        new BadRequestException("invalid", "param"));

    Assert.Equal(HttpStatusCode.BadRequest, statusCode);
  }

  [Fact]
  public void MapExceptionToStatusCode_DdeiClientException_UsesItsStatusCode()
  {
    var statusCode = ExceptionHandlingMiddleware.MapExceptionToStatusCode(
        new DdeiClientException(HttpStatusCode.NotFound, new HttpRequestException()));

    Assert.Equal(HttpStatusCode.NotFound, statusCode);
  }

  [Fact]
  public void MapExceptionToStatusCode_UnknownException_MapsToInternalServerError()
  {
    var statusCode = ExceptionHandlingMiddleware.MapExceptionToStatusCode(new InvalidOperationException());

    Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
  }
}
