namespace CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums
{
    public enum TransferStatus
    {
        Initiated, Queued, ListingFiles, InProgress,
        InProgressChunking, UploadingChunk, CompletingMultipart, // Granular for items
        Completed, PartiallyCompleted, Failed, Cancelled, Pending // Pending for items
    }
}