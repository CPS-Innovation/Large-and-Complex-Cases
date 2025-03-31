using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressArgFactory
{
  ListEgressWorkspacesArg CreateListEgressWorkspacesArg(string? name, int skip, int take);
  GetWorkspaceMaterialArg CreateGetWorkspaceMaterialArg(string caseId, int page, int count, string? folderId);
}