using System.ComponentModel.DataAnnotations;

namespace CPS.ComplexCases.API.Durable.Payloads;

public class TransferMaterialOrchestrationPayload(Guid operationIdRoot, string source, string destination)
{
  [Required]
  public Guid OperationIdRoot { get; set; } = operationIdRoot;
  [Required]
  public string Source { get; set; } = source;
  [Required]
  public string Destination { get; set; } = destination;
}