using System.Reflection;
using CPS.ComplexCases.API.Attributes;
using CPS.ComplexCases.API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Functions;

public static class Status
{
  [Function(nameof(Status))]
  public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req, [HttpTelemetry] object leaveThisInPlace)
  {
    return Assembly.GetExecutingAssembly().CurrentStatus();
  }
}