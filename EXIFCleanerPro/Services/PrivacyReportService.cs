using System.Net;
using System.Text;

namespace EXIFCleanerPro.Services;

internal sealed class PrivacyReportService : IPrivacyReportService
{
    public Task WriteHtmlAsync(string outputPath, PrivacyReportData report, CancellationToken cancellationToken)
    {
        string html = Render(report);
        return File.WriteAllTextAsync(outputPath, html, new UTF8Encoding(false), cancellationToken);
    }

    internal static string Render(PrivacyReportData report)
    {
        static string Encode(string value) => WebUtility.HtmlEncode(value);

        StringBuilder findings = new();
        foreach (PrivacyFinding finding in report.Assessment.Findings)
        {
            findings.Append("<li><strong>")
                .Append(Encode(finding.Title))
                .Append("</strong><span>")
                .Append(Encode(finding.Category.ToString()))
                .Append(" · +")
                .Append(finding.Points)
                .Append(" points</span><p>")
                .Append(Encode(finding.Explanation))
                .Append("</p></li>");
        }

        if (findings.Length == 0)
        {
            findings.Append("<li><strong>No sensitive findings</strong><p>The inspected metadata did not match the current privacy rules.</p></li>");
        }

        string verification = report.Comparison is null
            ? "<p class=\"muted\">No post-clean verification has been run for this image.</p>"
            : $"<div class=\"verification {(report.Comparison.VerificationPassed ? "pass" : "fail")}\"><strong>{Encode(report.Comparison.VerificationPassed ? "Verified clean" : "Verification warning")}</strong><p>{Encode(report.Comparison.EntryCountSummary)}</p><p>{Encode(report.Comparison.SensitiveCountSummary)}</p></div>";

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <title>Privacy report — {{Encode(report.FileName)}}</title>
              <style>
                :root{font-family:Segoe UI,system-ui,sans-serif;color:#17252a;background:#eef4f4}body{margin:0;padding:32px}.page{max-width:760px;margin:auto;background:white;border:1px solid #cad7d7;border-radius:16px;padding:32px;box-shadow:0 8px 30px #16383c18}h1{margin:0 0 6px}.muted{color:#607174}.score{display:flex;align-items:baseline;gap:12px;margin:28px 0;padding:20px;border-radius:12px;background:#f3f7f7}.score b{font-size:42px;color:#157f8c}.score span{font-size:20px;font-weight:600}ul{padding:0;list-style:none}li{border-top:1px solid #dce5e5;padding:16px 0}li span{float:right;color:#607174}li p{margin:7px 0 0;color:#45585b}.verification{margin-top:24px;padding:18px;border-radius:12px}.verification.pass{background:#e7f6ec;color:#175b31}.verification.fail{background:#fff0ee;color:#8a2e26}.verification p{margin:6px 0 0}footer{margin-top:28px;font-size:12px;color:#607174}
              </style>
            </head>
            <body><main class="page">
              <h1>Photo privacy report</h1>
              <p class="muted">{{Encode(report.FileName)}} · {{Encode(report.FilePath)}}</p>
              <section class="score"><b>{{report.Assessment.Score}}</b><span>/100 · {{Encode(report.Assessment.Level.ToString())}} risk</span></section>
              <h2>Detected privacy findings</h2>
              <ul>{{findings}}</ul>
              <h2>Cleaning verification</h2>
              {{verification}}
              <footer>Generated locally by EXIFCleaner Pro on {{Encode(report.GeneratedAt.ToLocalTime().ToString("g"))}}. A score reflects the app's current rules and is not a guarantee that an image contains no identifying visual content.</footer>
            </main></body>
            </html>
            """;
    }
}
