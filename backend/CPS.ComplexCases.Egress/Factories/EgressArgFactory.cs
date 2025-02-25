using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public class EgressArgFactory : IEgressArgFactory
{
  public FindWorkspaceArg CreateFindWorkspaceArg(string? name)
  {
    return new FindWorkspaceArg
    {
      Name = name
    };
  }

  public GetCaseMaterialArg CreateGetCaseMaterialArg(string caseId, int page, int count, string? folderId)
  {
    return new GetCaseMaterialArg
    {
      CaseId = caseId,
      Page = page,
      Count = count,
      FolderId = folderId
    };
  }
}