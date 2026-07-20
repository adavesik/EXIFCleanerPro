using EXIFCleanerPro.Services;

namespace EXIFCleanerPro.Tests;

public sealed class MetadataDisplayPolicyTests
{
    [Theory]
    [InlineData("ICC Profile", "Red TRC", "0.0, 0.0000763")]
    [InlineData("JPEG", "Quantization Table", "table data")]
    public void TechnicalTablesAreAdvancedOnly(string group, string tag, string value)
    {
        Assert.True(MetadataDisplayPolicy.IsAdvancedOnly(group, tag, value));
    }

    [Fact]
    public void UnusuallyLongValuesAreAdvancedOnly()
    {
        Assert.True(MetadataDisplayPolicy.IsAdvancedOnly("Other", "Raw data", new string('x', 241)));
    }

    [Fact]
    public void UsefulOrdinaryValuesRemainVisible()
    {
        Assert.False(MetadataDisplayPolicy.IsAdvancedOnly("Exif IFD0", "Model", "Canon EOS R6"));
    }

    [Fact]
    public void IccProfileIsPrivacyNeutral()
    {
        Assert.True(MetadataDisplayPolicy.IsPrivacyNeutralGroup("ICC Profile"));
        Assert.False(MetadataDisplayPolicy.IsPrivacyNeutralGroup("Exif IFD0"));
    }
}
