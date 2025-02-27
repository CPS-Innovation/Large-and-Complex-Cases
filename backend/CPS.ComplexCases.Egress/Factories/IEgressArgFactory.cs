using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressArgFactory
{
  FindWorkspaceArg CreateFindWorkspaceArg(string? name);
  GetWorkspaceMaterialArg CreateGetWorkspaceMaterialArg(string workspaceId, int page, int count, string? folderId);
  GetWorkspaceDocumentArg CreateGetWorkspaceDocumentArg(string workspaceId, string fileId);
}