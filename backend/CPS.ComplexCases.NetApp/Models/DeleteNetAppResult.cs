namespace CPS.ComplexCases.NetApp.Models;

public record DeleteNetAppResult(bool Success, int KeysDeleted, string? ErrorMessage, int? ErrorStatusCode);
