using EXIFCleanerPro.Services;

namespace EXIFCleanerPro.Tests;

public sealed class PrivacyReportServiceTests
{
    [Fact]
    public void RenderEscapesFileDataAndIncludesVerification()
    {
        PrivacyAssessment assessment = new(
            45,
            PrivacyRiskLevel.High,
            [new PrivacyFinding("gps", "Precise location", "Coordinates found.", PrivacyCategory.Location, 45, 2)],
            [new PrivacyCategoryRisk(PrivacyCategory.Location, 45, PrivacyRiskLevel.High)]);
        MetadataComparison comparison = new(3, 2, 2, 0, 2, []);
        PrivacyReportData report = new(
            "photo<script>.jpg",
            "C:\\photos\\photo<script>.jpg",
            assessment,
            comparison,
            new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.Zero));

        string html = PrivacyReportService.Render(report);

        Assert.Contains("photo&lt;script&gt;.jpg", html, StringComparison.Ordinal);
        Assert.DoesNotContain("photo<script>.jpg", html, StringComparison.Ordinal);
        Assert.Contains("Verified clean", html, StringComparison.Ordinal);
        Assert.Contains("45", html, StringComparison.Ordinal);
    }
}
