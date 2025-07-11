using Microsoft.AspNetCore.Mvc;

namespace CPS.ComplexCases.FileTransfer.API.Models.Results;

public class EgressPermissionExceptionResult : JsonResult
{
    public EgressPermissionExceptionResult(string message) : base(message)
    {
        StatusCode = 403;
    }
}