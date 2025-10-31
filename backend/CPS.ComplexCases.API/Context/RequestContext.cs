using CPS.ComplexCases.API.Exceptions;

namespace CPS.ComplexCases.API.Context;

public class RequestContext(Guid correlationId, string? cmsAuthValues, string? username)
{
    public Guid CorrelationId { get; set; } = correlationId;
    private string? InternalUsername { get; set; } = username;
    private string? InternalCmsAuthValues { get; set; } = cmsAuthValues;

    public string Username
    {
        get => InternalUsername
            // If the calling code is asking for a username and it isn't there then they are doing something wrong
            ?? throw new ArgumentNullException(nameof(Username), "Username is not set");
    }

    public string CmsAuthValues
    {

        get => InternalCmsAuthValues
            // If the calling code is asking for CmsAuthValues and it isn't there then they've not logged into CMS
            ?? throw new CpsAuthenticationException();
    }
}

