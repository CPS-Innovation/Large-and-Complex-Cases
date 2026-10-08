using CPS.ComplexCases.API.Domain.Models;

namespace CPS.ComplexCases.API.Tests.Unit.Domain.Models;

public class SecurityGroupTests
{
    private static SecurityGroup CreateSecurityGroup(List<string>? entryPrefixes) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = "Test Security Group",
        BucketName = "bucket-1",
        VolumeUuid = Guid.NewGuid(),
        EntryPrefixes = entryPrefixes
    };

    [Fact]
    public void NormalisedEntryPrefixes_TrimsSlashesAndDropsBlanksAndDuplicates()
    {
        var securityGroup = CreateSecurityGroup(["RCF/", "/RCB", " ", "RCW/Cardiff/", "rcf"]);

        Assert.Equal(["RCF/", "RCB/", "RCW/Cardiff/"], securityGroup.NormalisedEntryPrefixes);
    }

    [Fact]
    public void NormalisedEntryPrefixes_WhenNotConfigured_IsEmpty()
    {
        var securityGroup = CreateSecurityGroup(null);

        Assert.Empty(securityGroup.NormalisedEntryPrefixes);
    }

    [Fact]
    public void NormalisedEntryPrefixes_ReturnsTheSameInstanceOnRepeatedAccess()
    {
        var securityGroup = CreateSecurityGroup(["RCF/", "RCW/Cardiff/"]);

        Assert.Same(securityGroup.NormalisedEntryPrefixes, securityGroup.NormalisedEntryPrefixes);
    }
}
