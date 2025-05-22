using CPS.ComplexCases.ActivityLog.Attributes;

namespace CPS.ComplexCases.ActivityLog.Enums
{
    public enum ActionType
    {
        [AlternateValue("TRANSFER_INITIATED")]
        TransferInitiated,
        [AlternateValue("TRANSFER_STATUS_UPDATED")]
        TransferStatusUpdated,
        [AlternateValue("TRANSFER_CANCELLED")]
        TransferCancelled,
        [AlternateValue("TRANSFER_RETRY_INITIATED")]
        TransferRetryInitiated,
        [AlternateValue("TRANSFER_COMPLETED")]
        TransferCompleted
    }
}