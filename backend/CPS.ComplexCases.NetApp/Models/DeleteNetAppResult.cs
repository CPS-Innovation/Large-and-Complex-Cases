namespace CPS.ComplexCases.NetApp.Models;

public record DeleteNetAppResult(bool Success, bool WasFound, int KeysDeleted, string? ErrorMessage, int? ErrorStatusCode);
