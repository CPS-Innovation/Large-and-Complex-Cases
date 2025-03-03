using CPS.ComplexCases.API.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Durable.Activity;

public class InitiateTransferMaterial
{

  public InitiateTransferMaterial()
  {

  }

  [Function(nameof(InitiateTransferMaterial))]
  public async Task Run([ActivityTrigger] TransferMaterialOrchestrationPayload payload)
  {


    /*
      IDocumentTransferClient downloadClient = _clientSwitch.GetDownloadClient(sourcePath);
      IDocumentTransferClient uploadClient = _clientSwitch.GetUploadClient(sourcePath);

      var stream = await downloadClient.Download(source);
      await uploadClient(destination, stream);
    */

    // remove doc from Egress if egress path?
  }
}