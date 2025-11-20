using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using CPS.ComplexCases.Common.Extensions;

namespace CPS.ComplexCases.Common.Functions;

public static class StatusFunction
{
    public static IActionResult GetStatus(Assembly executingAssembly)
    {
        return executingAssembly.CurrentStatus();
    }
}