namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

public class SizeConfig
{
    public const string SectionName = "SizeConfig";

    public int ChunkSizeBytes { get; set; } = 8 * 1024 * 1024; // default to 8 MB
    public int MinMultipartSizeBytes { get; set; } = 5 * 1024 * 1024; // Default to 5 MB

    // Files per batch and concurrent part uploads per file stay inside the Egress Polly
    // concurrency limit of 30. At 12 files x 2 parts that is 24 concurrent chunk PATCHes.
    public int BatchSize { get; set; } = 12;
    public int MaxConcurrentPartUploads { get; set; } = 2;

    public int MaxOrchestratorRetries { get; set; } = 3; // default to 3

    // The orchestrator retry pass runs at lower concurrency than the first pass: a whole file retry
    // re-initiates and re-uploads every part, so retrying many files at once amplifies load on an
    // already-erroring Egress. Keep this at or below BatchSize.
    public int RetryBatchSize { get; set; } = 2;

    // Retry policy for the pre-flight Egress destination folder pre creation activity
    public int FolderPreCreateRetryAttempts { get; set; } = 3;
    public int FolderPreCreateFirstRetryIntervalSeconds { get; set; } = 5;
    public double FolderPreCreateBackoffCoefficient { get; set; } = 2.0;

    // Polling window for source-file availability before transfer fan-out.
    // 5 attempts including the first, 10s apart, gives a 50s maximum wait.
    public int SourceValidationRetryAttempts { get; set; } = 5;
    public int SourceValidationRetryIntervalSeconds { get; set; } = 10;
}
