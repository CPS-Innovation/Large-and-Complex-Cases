namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public interface IBatchFileItem
{
    string SourceKey { get; }
    string DestinationPrefix { get; }
    string DestinationFileName { get; }
}
