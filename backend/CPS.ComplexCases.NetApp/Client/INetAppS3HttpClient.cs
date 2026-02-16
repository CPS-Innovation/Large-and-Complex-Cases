using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.NetApp.Client;

public interface INetAppS3HttpClient
{
    Task<HeadObjectResponseDto> GetHeadObjectAsync(GetHeadObjectArg arg);
}