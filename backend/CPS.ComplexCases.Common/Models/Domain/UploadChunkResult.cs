using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.Common.Models.Domain;

public class UploadChunkResult
{
    public TransferDirection TransferDirection { get; set; }
    public string? ETag { get; set; }
    public int? PartNumber { get; set; }

    public UploadChunkResult(TransferDirection transferDirection, string? eTag = null, int? partNumber = null)
    {
        TransferDirection = transferDirection;
        ETag = eTag;
        PartNumber = partNumber;
    }
}