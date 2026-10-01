namespace CPS.ComplexCases.Common.Models.Domain;

/// <summary>
/// Outcome of a pre-flight source file probe. <see cref="SizeBytes"/> is null when the file exists
/// but the provider did not report a size, so an unknown size must not be treated as zero.
/// </summary>
public sealed record FileProbeResult(bool Exists, long? SizeBytes)
{
    public static readonly FileProbeResult NotFound = new(false, null);

    public static FileProbeResult Found(long? sizeBytes) => new(true, sizeBytes);
}
