using System.Net;

namespace CPS.ComplexCases.NetApp.Models.Dto;

public class HeadObjectResponseDto
{
    public string ETag { get; set; } = null!;
    public HttpStatusCode StatusCode { get; set; }
    public long ContentLength { get; set; }
}