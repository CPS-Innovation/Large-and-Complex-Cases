
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.Data.Services;

public interface ICaseMetadataService
{
  Task CreateEgressConnectionAsync(CreateEgressConnectionDto createEgressConnectionDto);
}