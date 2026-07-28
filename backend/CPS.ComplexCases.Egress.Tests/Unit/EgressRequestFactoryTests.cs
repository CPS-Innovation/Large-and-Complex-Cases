using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Tests.Unit;

public class EgressRequestFactoryTests
{
  private readonly EgressRequestFactory _sut = new();

  [Theory]
  [InlineData("Op Test", "Op%20Test")]
  [InlineData("Op&Test", "Op%26Test")]
  [InlineData("Op=Test", "Op%3DTest")]
  [InlineData("Op+Test", "Op%2BTest")]
  [InlineData("Op/Test", "Op%2FTest")]
  [InlineData("Op?Test", "Op%3FTest")]
  [InlineData("Op#Test", "Op%23Test")]
  public void ListWorkspacesRequest_WhenNameContainsReservedCharacters_PercentEncodesNameInQueryString(
    string name,
    string expectedEncodedName)
  {
    var arg = new ListEgressWorkspacesArg
    {
      Skip = 0,
      Take = 10,
      Name = name
    };

    using var request = _sut.ListWorkspacesRequest(arg, "token");

    Assert.Equal(
      $"/api/v1/workspaces?view=full&skip=0&limit=10&name={expectedEncodedName}",
      request.RequestUri?.ToString());
  }

  [Fact]
  public void ListWorkspacesRequest_WhenNameIsNull_OmitsNameQueryParameter()
  {
    var arg = new ListEgressWorkspacesArg
    {
      Skip = 5,
      Take = 25,
      Name = null
    };

    using var request = _sut.ListWorkspacesRequest(arg, "token");

    Assert.Equal(
      "/api/v1/workspaces?view=full&skip=5&limit=25",
      request.RequestUri?.ToString());
  }
}
