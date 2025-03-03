
namespace CPS.ComplexCases.API.Domain
{
  public class TransferMaterialStatus
  {
    public required string FilePath { get; set; }
    public required OrchestrationStatus Status { get; set; }
  }
}