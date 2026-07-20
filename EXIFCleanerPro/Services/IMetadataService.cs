namespace EXIFCleanerPro.Services;

internal interface IMetadataService
{
    Task<MetadataResult> ReadAsync(string filePath, CancellationToken cancellationToken);
}
