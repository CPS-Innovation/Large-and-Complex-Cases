using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class InitiateTransfer
{
    private readonly ILogger<InitiateTransfer> _logger;

    public InitiateTransfer(ILogger<InitiateTransfer> logger)
    {
        _logger = logger;
    }

    [Function(nameof(InitiateTransfer))]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "transfer")] HttpRequest req,
    [DurableClient] DurableTaskClient orchestrationClient)
    {
        var egressConnectionRequest = await ValidatorHelper.GetJsonBody<TransferRequest, TransferRequestValidator>(req);

        if (!egressConnectionRequest.IsValid)
        {
            return new BadRequestObjectResult(egressConnectionRequest.ValidationErrors);
        }
    }
}