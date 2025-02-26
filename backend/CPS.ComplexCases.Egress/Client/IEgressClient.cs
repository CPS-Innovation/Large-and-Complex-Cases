
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;

namespace CPS.ComplexCases.Egress.Client;

public interface IEgressClient
{
  Task<IEnumerable<FindWorkspaceDto>> FindWorkspace(FindWorkspaceArg workspace, string email);
  Task<GetCaseMaterialDto> GetCaseMaterial(GetWorkspaceMaterialArg arg);
  Task<Stream> GetCaseDocument(GetWorkspaceDocumentArg arg);
}
