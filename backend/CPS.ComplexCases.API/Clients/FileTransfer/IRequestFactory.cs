namespace CPS.ComplexCases.API.Clients.FileTransfer;

public interface IRequestFactory
{
    HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, HttpContent? content = null);
}