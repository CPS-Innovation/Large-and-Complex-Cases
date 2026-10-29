using CPS.ComplexCases.Common.Helpers;

namespace CPS.ComplexCases.Common.Tests.Unit.Helpers;

public class NetAppTemplatePathHelperTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("/", "")]
    [InlineData("Area1", "Area1/")]
    [InlineData("Area1/TeamA/", "Area1/TeamA/")]
    [InlineData("/Area1/TeamA", "Area1/TeamA/")]
    public void NormaliseParentFolderPath_ReturnsExpectedPath(string? input, string expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.NormaliseParentFolderPath(input));
    }

    [Theory]
    [InlineData("_templates/", true)]
    [InlineData("_templates", true)]
    [InlineData("_TEMPLATES/Standard/", true)]
    [InlineData("Area1/_templates/", true)]
    [InlineData("Area1/_templates/Standard/Sub/", true)]
    [InlineData("Area1/TeamA/", false)]
    [InlineData("Area1/my_templates/", false)]
    [InlineData("Area1/_templatesOld/", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsWithinTemplatesFolder_DetectsTemplatesSegmentAtAnyDepth(string? path, bool expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.IsWithinTemplatesFolder(path));
    }

    [Theory]
    [InlineData(null, "_templates/")]
    [InlineData("", "_templates/")]
    [InlineData("Area1/TeamA/", "Area1/TeamA/_templates/")]
    [InlineData("Area1/TeamA", "Area1/TeamA/_templates/")]
    public void GetTemplatesFolderPath_ReturnsTemplatesFolderUnderParent(string? parent, string expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.GetTemplatesFolderPath(parent));
    }

    [Theory]
    [InlineData("_templates/Standard/", "", true)]
    [InlineData("_TEMPLATES/Standard/", null, true)]
    [InlineData("Area1/TeamA/_templates/Standard/", "Area1/TeamA/", true)]
    [InlineData("area1/teama/_templates/Standard/", "Area1/TeamA/", true)]
    [InlineData("Area1/TeamA/_templates/Standard/Sub/", "Area1/TeamA/", true)]
    [InlineData("Area1/TeamA/_templates/", "Area1/TeamA/", false)]
    [InlineData("_templates/", "", false)]
    [InlineData("Area1/TeamB/_templates/Standard/", "Area1/TeamA/", false)]
    [InlineData("Area1/TeamA/_templates/Standard/", "Area1/", false)]
    [InlineData("Area1/TeamA/_templates/Standard/", "", false)]
    [InlineData("_templates/Standard/", "Area1/TeamA/", false)]
    [InlineData("", "", false)]
    [InlineData(null, "", false)]
    public void IsTemplateUnderParent_RequiresTemplatesFolderDirectlyUnderParent(string? template, string? parent, bool expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.IsTemplateUnderParent(template, parent));
    }

    [Theory]
    [InlineData(null, "Case-1", "Case-1/")]
    [InlineData("", "Case-1", "Case-1/")]
    [InlineData("Area1/TeamA/", "Case-1", "Area1/TeamA/Case-1/")]
    [InlineData("Area1/TeamA", "Case-1", "Area1/TeamA/Case-1/")]
    public void BuildDestinationPath_CombinesParentAndCaseName(string? parent, string caseName, string expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.BuildDestinationPath(parent, caseName));
    }

    [Theory]
    [InlineData("Case-1/", "")]
    [InlineData("Area1/TeamA/Case-1/", "Area1/TeamA/")]
    [InlineData("Area1/Case-1", "Area1/")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void GetParentFolderPath_RemovesLastSegment(string? destination, string expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.GetParentFolderPath(destination));
    }

    [Theory]
    [InlineData("Area1/TeamA/_templates/Standard/", "Standard")]
    [InlineData("_templates/Complex", "Complex")]
    public void GetFolderName_ReturnsLastSegment(string path, string expected)
    {
        Assert.Equal(expected, NetAppTemplatePathHelper.GetFolderName(path));
    }
}
