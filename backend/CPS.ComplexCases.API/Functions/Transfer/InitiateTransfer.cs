using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Validators.Requests;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class InitiateTransfer(ILogger<InitiateTransfer> logger, ITransferClient transferClient)
{
    private readonly ILogger<InitiateTransfer> _logger = logger;
    private readonly ITransferClient _transferClient = transferClient;

    [Function(nameof(InitiateTransfer))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "filetransfer/initiate")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var transferRequest = await ValidatorHelper.GetJsonBody<InitiateTransferRequest, InitiateTransferRequestValidator>(req);

        if (!transferRequest.IsValid)
        {
            return new BadRequestObjectResult(transferRequest.ValidationErrors);
        }



    }
}