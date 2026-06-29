using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Models.Results;

namespace CPS.ComplexCases.API.Domain.Models;

public record DisconnectConnectionConfig(
    Func<int, Task<ClearFolderPathResult>> ClearConnection,
    CaseMetadataState MissingConnectionState,
    ActionType ActivityLogAction,
    Func<int, string> NotFoundMessage,
    Func<int, string> ActiveTransferMessage,
    Func<int, string> MissingConnectionMessage,
    string ActivityLogFailureMessage
);