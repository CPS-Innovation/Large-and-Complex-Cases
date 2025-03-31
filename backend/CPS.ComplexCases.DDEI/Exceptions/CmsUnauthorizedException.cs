namespace CPS.ComplexCases.DDEI.Exceptions;

[Serializable]
public class CmsUnauthorizedException : Exception
{
  public CmsUnauthorizedException()
      : base($"Unauthorized access to CMS")
  {
  }
}