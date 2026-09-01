using System.Reflection;
using CPS.ComplexCases.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.Common.Functions;

public static class StatusFunction
{
    public static IActionResult GetStatus(Assembly executingAssembly)
    {
        return executingAssembly.CurrentStatus();
    }
}