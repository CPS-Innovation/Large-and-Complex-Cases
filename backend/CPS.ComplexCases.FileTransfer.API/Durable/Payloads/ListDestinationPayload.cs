namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public record ListDestinationPayload(string WorkspaceId, string DestinationPath);