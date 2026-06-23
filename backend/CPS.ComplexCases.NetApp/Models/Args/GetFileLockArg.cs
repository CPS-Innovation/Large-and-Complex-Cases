namespace CPS.ComplexCases.NetApp.Models.Args;

public class GetFileLockArg : BaseNetAppArg
{
    public required Guid VolumeUuid { get; set; }
    public required string FilePath { get; set; }
}