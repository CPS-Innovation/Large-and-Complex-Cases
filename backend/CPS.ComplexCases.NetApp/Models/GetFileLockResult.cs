namespace CPS.ComplexCases.NetApp.Models;

public record GetFileLockResult(bool IsLocked, string? LockedBy, string? ErrorMessage, int? ErrorStatusCode);