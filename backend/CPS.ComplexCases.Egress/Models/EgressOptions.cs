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
}
