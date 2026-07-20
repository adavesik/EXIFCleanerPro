namespace EXIFCleanerPro.Services;

internal interface IPrivacyReportService
{
    Task WriteHtmlAsync(string outputPath, PrivacyReportData report, CancellationToken cancellationToken);
}
