using CPS.ComplexCases.Common.Attributes;

namespace CPS.ComplexCases.ActivityLog.Enums
{
    public enum ActionType
    {
        [AlternateValue("TRANSFER_INITIATED")]
        TransferInitiated,
        [AlternateValue("TRANSFER_COMPLETED")]
        TransferCompleted,
        [AlternateValue("TRANSFER_FAILED")]
        TransferFailed,
        [AlternateValue("CONNECTION_TO_EGRESS")]
        ConnectionToEgress,
        [AlternateValue("CONNECTION_TO_NETAPP")]
        ConnectionToNetApp
    }
}