namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

public class SizeConfig
{
    public const string SectionName = "SizeConfig";

    public int ChunkSizeBytes { get; set; } = 8 * 1024 * 1024; // default to 8 MB
    public int MinMultipartSizeBytes { get; set; } = 5 * 1024 * 1024; // Default to 5 MB

    // Files per batch and concurrent part uploads per file are kept conservative for large (~1 GB)
    // files: at 4 files x 2 parts that is 8 concurrent chunk PATCH'
    public int BatchSize { get; set; } = 4; // Default to 4 files per batch
    public int MaxConcurrentPartUploads { get; set; } = 2;

    public int MaxOrchestratorRetries { get; set; } = 3; // default to 3

    // The orchestrator retry pass runs at lower concurrency than the first pass: a whole file retry
    // re-initiates and re-uploads every part, so retrying many files at once amplifies load on an
    // already-erroring Egress. Keep this at or below BatchSize.
    public int RetryBatchSize { get; set; } = 2;
}