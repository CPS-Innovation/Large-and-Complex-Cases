using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressArgFactory
{
  FindWorkspaceArg CreateFindWorkspaceArg(string name);
  GetCaseMaterialArg CreateGetCaseMaterialArg(string caseId, int page, int count, string? folderId);
}