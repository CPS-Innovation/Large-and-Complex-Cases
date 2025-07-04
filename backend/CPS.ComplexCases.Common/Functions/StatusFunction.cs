using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Models.Domain;

namespace CPS.ComplexCases.Common.Functions;

public static class StatusFunction
{
    public static IActionResult GetStatus(Assembly executingAssembly)
    {
        return executingAssembly.CurrentStatus();
    }
}