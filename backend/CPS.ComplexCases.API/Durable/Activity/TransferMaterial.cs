using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.NetApp.Client;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Durable.Activity;

public class TransferMaterial
{
  private readonly IEgressClient _egressClient;
  private readonly INetAppClient _netAppClient;

  public TransferMaterial(IEgressClient egressClient, INetAppClient netAppClient)
  {
    _egressClient = egressClient;
    _netAppClient = netAppClient;
  }

  [Function(nameof(TransferMaterial))]
  public async Task Run([ActivityTrigger] TransferMaterialOrchestrationPayload payload)
  {
    // download doc from Egress
    var egressPayload = new GetCaseDocumentArg
    {
      CaseId = payload.WorkspaceId,
      FileId = payload.DocumentId
    };

    using var stream = await _egressClient.GetCaseDocument(egressPayload);


    // upload doc to NetApp s3
  }
}