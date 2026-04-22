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
        ConnectionToNetApp,
        [AlternateValue("DISCONNECTION_FROM_NETAPP")]
        DisconnectionFromNetApp,
        [AlternateValue("FOLDER_CREATED")]
        FolderCreated,
        [AlternateValue("FOLDER_DELETED")]
        FolderDeleted,
        [AlternateValue("MATERIAL_DELETED")]
        MaterialDeleted,
        [AlternateValue("FOLDER_AND_MATERIAL_DELETED")]
        FolderAndMaterialDeleted,
        [AlternateValue("FOLDER_RENAMED")]
        FolderRenamed,
        [AlternateValue("FOLDER_COPIED")]
        FolderCopied,
        [AlternateValue("FOLDER_MOVED")]
        FolderMoved,
        [AlternateValue("MATERIAL_RENAMED")]
        MaterialRenamed,
        [AlternateValue("MATERIAL_MOVED")]
        MaterialMoved,
        [AlternateValue("MATERIAL_COPIED")]
        MaterialCopied,
        [AlternateValue("FILE_MOVED")]
        FileMoved
    }
}