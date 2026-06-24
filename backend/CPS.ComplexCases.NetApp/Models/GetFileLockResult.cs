namespace CPS.ComplexCases.NetApp.Models;

public record GetFileLockResult(bool IsLocked, string? LockedBy, string[]? Types, string Message);