namespace CPS.ComplexCases.NetApp.Models;

public record MaterialRenameResult(bool Success, bool WasFound, int KeysRenamed, string? ErrorMessage, int? ErrorStatusCode);