using CPS.ComplexCases.API.Validators;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.API.Handlers;

public interface IInitializationHandler
{
  public Task<ValidateTokenResult> Initialize(HttpRequest request);
}