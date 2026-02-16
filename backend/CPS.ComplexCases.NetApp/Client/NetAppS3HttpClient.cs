using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Services;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppS3HttpClient(HttpClient httpClient, IS3CredentialService s3CredentialService, IOptions<NetAppOptions> options) : INetAppS3HttpClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IS3CredentialService _s3CredentialService = s3CredentialService;
    private readonly NetAppOptions _options = options.Value;
    private const string UnsignedPayload = "UNSIGNED-PAYLOAD";

    public async Task<HeadObjectResponseDto> GetHeadObjectAsync(GetHeadObjectArg arg)
    {
        var key = $"{arg.BucketName}/{arg.ObjectKey}";
        var request = new HttpRequestMessage(HttpMethod.Head, key);
        await SignRequest(request, arg.BearerToken, key, string.Empty);
        var response = await _httpClient.SendAsync(request);

        var headObjectResponse = new HeadObjectResponseDto
        {
            StatusCode = response.StatusCode,
            ETag = response.Headers.ETag?.Tag.Unquote() ?? string.Empty
        };

        return headObjectResponse;
    }

    private async Task SignRequest(HttpRequestMessage request, string bearerToken, string key, string payload)
    {
        var (accessKey, secretKey) = await _s3CredentialService.GetCredentialKeysAsync(bearerToken);

        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'");
        var dateStamp = now.ToString("yyyyMMdd");
        var baseAddress = _httpClient.BaseAddress ?? throw new InvalidOperationException("HttpClient must have a BaseAddress configured.");
        var host = baseAddress.IsDefaultPort ? baseAddress.Host : baseAddress.Authority;
        var payloadHash = string.IsNullOrEmpty(payload) ? UnsignedPayload : Hash(payload);

        request.Headers.Add("host", host);
        request.Headers.Add("x-amz-date", amzDate);
        request.Headers.Add("x-amz-content-sha256", payloadHash);

        // Include prefixing slash in canonical URI as per AWS requirements
        var canonicalUri = "/" + string.Join("/", key.Split('/').Select(Uri.EscapeDataString));
        var canonicalQueryString = string.Empty;
        var canonicalHeaders = $"host:{host}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{amzDate}\n";
        var signedHeaders = "host;x-amz-content-sha256;x-amz-date";

        var canonicalRequest = $"{request.Method}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

        var algorithm = "AWS4-HMAC-SHA256";
        var credentialScope = $"{dateStamp}/{_options.RegionName}/s3/aws4_request";
        var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{Hash(canonicalRequest)}";

        var signingKey = GetSignatureKey(secretKey!, dateStamp, _options.RegionName, "s3");
        var signature = ToHexString(HmacSha256(signingKey, stringToSign));

        var authorizationHeader = $"{algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
    }

    private static string Hash(string data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return ToHexString(hash);
    }

    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
    {
        var kDate = HmacSha256(Encoding.UTF8.GetBytes("AWS4" + key), dateStamp);
        var kRegion = HmacSha256(kDate, regionName);
        var kService = HmacSha256(kRegion, serviceName);
        var kSigning = HmacSha256(kService, "aws4_request");
        return kSigning;
    }

    private static string ToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}