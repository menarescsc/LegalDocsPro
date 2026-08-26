using System.Text;
using FluentAssertions;
using LegalDocsPro.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LegalDocsPro.Application.Tests.Storage;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"legaldocspro-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["FileStorage:BasePath"]).Returns(_testDirectory);

        _service = new LocalFileStorageService(configuration.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    // ── SaveAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ReturnsGuidBasedFilename()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 test content");
        using var stream = new MemoryStream(content);

        var storedName = await _service.SaveAsync(stream, "contract.pdf");

        storedName.Should().EndWith(".pdf");
        // GUID format: 8-4-4-4-12 characters (without extension)
        var nameWithoutExt = Path.GetFileNameWithoutExtension(storedName);
        Guid.TryParse(nameWithoutExt, out _).Should().BeTrue("because filename should be a GUID");
    }

    [Fact]
    public async Task SaveAsync_StoresFileInConfiguredDirectory()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 test content");
        using var stream = new MemoryStream(content);

        var storedName = await _service.SaveAsync(stream, "document.pdf");

        var filePath = Path.Combine(_testDirectory, storedName);
        File.Exists(filePath).Should().BeTrue("because the file should be stored in the configured directory");
    }

    [Fact]
    public async Task SaveAsync_FileContentIsPreserved()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 specific content for verification");
        using var stream = new MemoryStream(content);

        var storedName = await _service.SaveAsync(stream, "test.pdf");

        var storedContent = await File.ReadAllBytesAsync(Path.Combine(_testDirectory, storedName));
        storedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task SaveAsync_MultipleFilesGetUniqueNames()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 content");

        using var stream1 = new MemoryStream(content);
        using var stream2 = new MemoryStream(content);

        var name1 = await _service.SaveAsync(stream1, "file1.pdf");
        var name2 = await _service.SaveAsync(stream2, "file2.pdf");

        name1.Should().NotBe(name2, "because each upload should get a unique GUID");
    }

    [Fact]
    public async Task SaveAsync_PreservesFileExtension()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 content");
        using var stream = new MemoryStream(content);

        var storedName = await _service.SaveAsync(stream, "report.PDF");

        storedName.Should().EndWith(".PDF");
    }

    // ── GetFilePath ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetFilePath_ReturnsFullPathForExistingFile()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 content");
        using var stream = new MemoryStream(content);
        var storedName = await _service.SaveAsync(stream, "test.pdf");

        var filePath = _service.GetFilePath(storedName);

        filePath.Should().NotBeNull();
        filePath.Should().StartWith(_testDirectory);
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void GetFilePath_ReturnsNullForNonExistentFile()
    {
        var filePath = _service.GetFilePath($"{Guid.NewGuid()}.pdf");

        filePath.Should().BeNull();
    }

    [Fact]
    public void GetFilePath_ReturnsNullForEmptyString()
    {
        _service.GetFilePath("").Should().BeNull();
    }

    [Fact]
    public void GetFilePath_ReturnsNullForNullInput()
    {
        _service.GetFilePath(null!).Should().BeNull();
    }

    [Fact]
    public void GetFilePath_RejectsPathTraversalAttempts()
    {
        // Attempt to escape the storage directory
        _service.GetFilePath("../../../etc/passwd").Should().BeNull();
        _service.GetFilePath("..\\..\\windows\\system32\\config").Should().BeNull();
        _service.GetFilePath("subdir/../../../etc/passwd").Should().BeNull();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesExistingFile()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 content");
        using var stream = new MemoryStream(content);
        var storedName = await _service.SaveAsync(stream, "test.pdf");

        await _service.DeleteAsync(storedName);

        File.Exists(Path.Combine(_testDirectory, storedName)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForNonExistentFile()
    {
        var act = () => _service.DeleteAsync($"{Guid.NewGuid()}.pdf");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void DeleteAsync_DoesNotThrowForPathTraversalAttempt()
    {
        var act = () => _service.DeleteAsync("../../../etc/passwd");

        act.Should().NotThrowAsync();
    }
}
