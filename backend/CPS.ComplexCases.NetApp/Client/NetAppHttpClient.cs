using System.Xml.Serialization;
using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppHttpClient : INetAppClient
{
    public async Task<bool> CreateBucketAsync(CreateBucketArg arg)
    {
        const string baseUrl = "http://4.250.60.78:9090";
        try
        {
            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("x-amz-date", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
                //client.DefaultRequestHeaders.Add("x-amz-region", "eu-west-2");
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                HttpResponseMessage response = await client.PutAsync(
                $"{baseUrl}/{arg.BucketName}", content: null
            );

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Bucket '{arg.BucketName}' created successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to create bucket. Status Code: {response.StatusCode}");
                    return false;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<S3Bucket?> FindBucketAsync(FindBucketArg arg)
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-amz-date", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
                client.DefaultRequestHeaders.Add("x-amz-region", "eu-west-2");

                var response = await client.GetAsync($"http://4.250.60.78:9090");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var serializer = new XmlSerializer(typeof(S3Bucket));
                    using var reader = new StringReader(content);
                    return (S3Bucket?)serializer.Deserialize(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public Task<S3AccessControlList?> GetACLForBucketAsync(string bucketName)
    {
        throw new NotImplementedException();
    }

    public Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<S3Bucket>> ListBucketsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> ListFoldersInBucketAsync(ListFoldersInBucketArg arg)
    {
        throw new NotImplementedException();
    }

    public Task<ListObjectsV2Response?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UploadObjectAsync(UploadObjectArg arg)
    {
        throw new NotImplementedException();
    }
}