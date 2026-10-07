namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public record DemoteSuccessfulItemsPayload(List<string> SourcePaths, string ErrorMessage);
