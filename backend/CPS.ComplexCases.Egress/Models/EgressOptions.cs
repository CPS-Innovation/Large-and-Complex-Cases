namespace CPS.ComplexCases.Egress.Models;

public class EgressOptions
{
  public const string ConfigKey = "Egress";
  public required string Username { get; set; }
  public required string Password { get; set; }
  public required string Url { get; set; }

  // Management calls (workspace/list/permission/token)
  public int ManagementTimeoutSeconds { get; set; } = 100;

  // Applies to a single chunk upload (8-10 MB). Normally seconds; 5 minutes absorbs slow links
  // without re-introducing the old 10-minute hang. Also reused by TransferFile as the per-read idle
  // timeout for streamed downloads, so a stalled read fails in minutes rather than at the function timeout.
  public int TransferTimeoutSeconds { get; set; } = 300;

  // Per-chunk retry budget for the Egress multipart PATCH. Egress returns intermittent, load-correlated
  // 500s on chunk uploads; retrying the single chunk lets a file ride out a short error window instead
  // of failing the whole file and forcing the orchestrator to re-upload every part.
  public int MaxChunkUploadAttempts { get; set; } = 4;

  // Base delay for the exponential-with-jitter backoff between chunk upload attempts.
  public int ChunkRetryBaseDelaySeconds { get; set; } = 2;

  // After creating a destination folder, Egress may not make it immediately visible to create-upload.
  // These bound the settle/retry loop that waits for the folder to appear before giving up.
  public int CreateUploadRetryAttempts { get; set; } = 3;
  public int CreateUploadSettleDelaySeconds { get; set; } = 1;
}
