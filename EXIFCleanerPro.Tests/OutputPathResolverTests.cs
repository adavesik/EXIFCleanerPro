using EXIFCleanerPro.Services;

namespace EXIFCleanerPro.Tests;

public sealed class OutputPathResolverTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(Path.GetTempPath(), $"ExifCleanerTests-{Guid.NewGuid():N}");

    public OutputPathResolverTests() => Directory.CreateDirectory(testDirectory);

    [Fact]
    public void CleanedCopyUsesSuffixAndAvoidsCollisions()
    {
        string input = CreateFile("photo.jpg");
        string firstCollision = CreateFile("photo-cleaned.jpg");

        string output = OutputPathResolver.GetCleanedCopyPath(input);

        Assert.Equal(Path.Combine(testDirectory, "photo-cleaned (2).jpg"), output);
        Assert.True(File.Exists(firstCollision));
    }

    [Fact]
    public void BackupUsesBackupSuffixAndAvoidsCollisions()
    {
        string input = CreateFile("photo.png");
        CreateFile("photo-backup.png");

        string output = OutputPathResolver.GetBackupPath(input);

        Assert.Equal(Path.Combine(testDirectory, "photo-backup (2).png"), output);
    }

    public void Dispose() => Directory.Delete(testDirectory, true);

    private string CreateFile(string name)
    {
        string path = Path.Combine(testDirectory, name);
        File.WriteAllText(path, name);
        return path;
    }
}
