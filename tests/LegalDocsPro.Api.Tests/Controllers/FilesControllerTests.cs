using System.Text;
using FluentAssertions;
using LegalDocsPro.Api.Controllers;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LegalDocsPro.Api.Tests.Controllers;

public class FilesControllerTests : IDisposable
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IContractRepository> _contractRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly FilesController _controller;
    private readonly string _tempDir;

    public FilesControllerTests()
    {
        _fileStorageMock = new Mock<IFileStorageService>();
        _contractRepositoryMock = new Mock<IContractRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new FilesController(
            _fileStorageMock.Object,
            _contractRepositoryMock.Object,
            _currentUserServiceMock.Object);

        _tempDir = Path.Combine(Path.GetTempPath(), $"legaldocspro-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch (IOException)
        {
            // FileStreamResult may still hold a lock on the file during test cleanup.
            // The OS will clean up the temp directory eventually.
        }
    }

    // ── Upload Validation ────────────────────────────────────────────────

    [Fact]
    public async Task Upload_NullFile_ReturnsBadRequest()
    {
        var request = new FileUploadRequest { File = null! };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        var file = CreateFormFile("test.pdf", new byte[0], "application/pdf");
        var request = new FileUploadRequest { File = file };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_FileExceeds10MB_ReturnsBadRequest()
    {
        var largeContent = new byte[11 * 1024 * 1024]; // 11 MB
        Array.Fill(largeContent, (byte)0x20);
        // Add PDF magic bytes at start
        largeContent[0] = 0x25; // %
        largeContent[1] = 0x50; // P
        largeContent[2] = 0x44; // D
        largeContent[3] = 0x46; // F

        var file = CreateFormFile("large.pdf", largeContent, "application/pdf");
        var request = new FileUploadRequest { File = file };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("File size exceeds the 10 MB limit.");
    }

    [Fact]
    public async Task Upload_NonPdfExtension_ReturnsBadRequest()
    {
        var pdfContent = CreateValidPdfContent();
        var file = CreateFormFile("document.txt", pdfContent, "application/pdf");
        var request = new FileUploadRequest { File = file };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Only PDF files are accepted.");
    }

    [Fact]
    public async Task Upload_NonPdfContentType_ReturnsBadRequest()
    {
        var pdfContent = CreateValidPdfContent();
        var file = CreateFormFile("document.pdf", pdfContent, "image/png");
        var request = new FileUploadRequest { File = file };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Only PDF files are accepted. Invalid content type.");
    }

    [Fact]
    public async Task Upload_WrongMagicBytes_ReturnsBadRequest()
    {
        var fakeContent = Encoding.UTF8.GetBytes("This is not a PDF file content");
        var file = CreateFormFile("document.pdf", fakeContent, "application/pdf");
        var request = new FileUploadRequest { File = file };

        var result = await _controller.UploadFile(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("File content does not match PDF format.");
    }

    [Fact]
    public async Task Upload_ValidPdf_ReturnsOkWithStoredName()
    {
        var pdfContent = CreateValidPdfContent();
        var file = CreateFormFile("contract.pdf", pdfContent, "application/pdf");
        var request = new FileUploadRequest { File = file };

        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), "contract.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf");

        var result = await _controller.UploadFile(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _fileStorageMock.Verify(s => s.SaveAsync(It.IsAny<Stream>(), "contract.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upload_FileExactlyAt10MB_Succeeds()
    {
        var exactContent = new byte[10 * 1024 * 1024]; // exactly 10 MB
        Array.Fill(exactContent, (byte)0x20);
        exactContent[0] = 0x25; // %
        exactContent[1] = 0x50; // P
        exactContent[2] = 0x44; // D
        exactContent[3] = 0x46; // F

        var file = CreateFormFile("exact.pdf", exactContent, "application/pdf");
        var request = new FileUploadRequest { File = file };

        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), "exact.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("guid.pdf");

        var result = await _controller.UploadFile(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Download Authorization ───────────────────────────────────────────

    [Fact]
    public async Task Download_StoredNameNotFound_ReturnsNotFound()
    {
        _fileStorageMock.Setup(s => s.GetFilePath("nonexistent.pdf")).Returns((string?)null);

        var result = await _controller.DownloadFile("nonexistent.pdf", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Download_NoContractReferencesFile_ReturnsNotFound()
    {
        var tempFile = CreateTempFile("file.pdf");
        _fileStorageMock.Setup(s => s.GetFilePath("file.pdf")).Returns(tempFile);
        _contractRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Contract>());

        var result = await _controller.DownloadFile("file.pdf", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Download_OwnerCanDownloadTheirContractDocument()
    {
        var storedName = "a1b2c3d4.pdf";
        var tempFile = CreateTempFile(storedName);
        var contract = CreateContract(createdBy: "user-alice", documentUrl: $"/uploads/{storedName}");

        _fileStorageMock.Setup(s => s.GetFilePath(storedName)).Returns(tempFile);
        _contractRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Contract> { contract });
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var result = await _controller.DownloadFile(storedName, CancellationToken.None);

        result.Should().BeOfType<FileStreamResult>();
    }

    [Fact]
    public async Task Download_NonOwnerCannotDownload_ReturnsNotFound()
    {
        var storedName = "a1b2c3d4.pdf";
        var tempFile = CreateTempFile(storedName);
        var contract = CreateContract(createdBy: "user-alice", documentUrl: $"/uploads/{storedName}");

        _fileStorageMock.Setup(s => s.GetFilePath(storedName)).Returns(tempFile);
        _contractRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Contract> { contract });
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-bob");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var result = await _controller.DownloadFile(storedName, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Download_AdminCanDownloadAnyDocument()
    {
        var storedName = "a1b2c3d4.pdf";
        var tempFile = CreateTempFile(storedName);
        var contract = CreateContract(createdBy: "user-alice", documentUrl: $"/uploads/{storedName}");

        _fileStorageMock.Setup(s => s.GetFilePath(storedName)).Returns(tempFile);
        _contractRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Contract> { contract });
        _currentUserServiceMock.Setup(s => s.UserId).Returns("admin-1");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

        var result = await _controller.DownloadFile(storedName, CancellationToken.None);

        result.Should().BeOfType<FileStreamResult>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string CreateTempFile(string fileName)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllBytes(filePath, CreateValidPdfContent());
        return filePath;
    }

    private static IFormFile CreateFormFile(string fileName, byte[]? content = null, string contentType = "application/pdf")
    {
        content ??= CreateValidPdfContent();
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static byte[] CreateValidPdfContent()
    {
        // Minimal valid PDF header
        var header = Encoding.UTF8.GetBytes("%PDF-1.4\n");
        var content = new byte[100];
        Array.Copy(header, content, header.Length);
        return content;
    }

    private static Contract CreateContract(string createdBy, int id = 42, string documentUrl = "/uploads/test.pdf")
    {
        var contract = new Contract("Title", "Description", documentUrl, null);
        contract.Id = id;
        contract.CreatedBy = createdBy;
        return contract;
    }
}
