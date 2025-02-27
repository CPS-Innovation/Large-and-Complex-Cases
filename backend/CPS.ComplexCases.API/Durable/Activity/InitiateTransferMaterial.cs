using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Durable.Activity;

public class InitiateTransferMaterial
{
  private readonly IEgressClient _egressClient;
  private readonly IEgressArgFactory _egressArgFactory;
  private readonly INetAppClient _netAppClient;
  private readonly INetAppArgFactory _netAppArgFactory;


  public InitiateTransferMaterial(IEgressClient egressClient, IEgressArgFactory egressArgFactory, INetAppClient netAppClient, INetAppArgFactory netAppArgFactory)
  {
    _egressClient = egressClient;
    _egressArgFactory = egressArgFactory;
    _netAppClient = netAppClient;
    _netAppArgFactory = netAppArgFactory;
  }

  [Function(nameof(InitiateTransferMaterial))]
  public async Task Run([ActivityTrigger] TransferMaterialOrchestrationPayload payload)
  {
    // download doc from Egress
    var egressPayload = _egressArgFactory.CreateGetWorkspaceDocumentArg(payload.WorkspaceId, payload.DocumentId);
    using var stream = await _egressClient.GetCaseDocument(egressPayload);


    // upload doc to NetApp s3
    await _netAppClient.UploadObjectAsync(_netAppArgFactory.CreateUploadObjectArg(payload.DestinationPath, payload.DocumentId, stream));


    // remove doc from Egress?
  }
}