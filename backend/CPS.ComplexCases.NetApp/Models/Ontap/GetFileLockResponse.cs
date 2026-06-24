namespace CPS.ComplexCases.NetApp.Models.Ontap;

public record GetFileLockResponse(bool IsLocked, string? LockType, string? ErrorMessage, int? ErrorStatusCode);