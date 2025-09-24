using CPS.ComplexCases.API.Domain.Response;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.API.Services;

public interface IInitService
{
    Task<InitResult> ProcessRequest(HttpRequest req, Guid correlationId, string? cc);
}