using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using CPS.ComplexCases.API.Extensions;

namespace CPS.ComplexCases.API.Functions;

public static class Status
{
  [Function(nameof(Status))]
  public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
  {
    return Assembly.GetExecutingAssembly().CurrentStatus();
  }
}