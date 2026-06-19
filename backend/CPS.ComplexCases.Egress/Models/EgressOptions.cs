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
  // without re-introducing the old 10-minute hang. Streamed downloads are not capped here -
  // their body read is governed by the orchestration CancellationToken.
  public int TransferTimeoutSeconds { get; set; } = 300;
}
