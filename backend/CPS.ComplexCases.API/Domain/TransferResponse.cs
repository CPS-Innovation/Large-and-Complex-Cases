namespace CPS.ComplexCases.API.Domain;

public class TransferResponse(Guid transferId)
{
  public Guid TransferId { get; set; } = transferId;
}