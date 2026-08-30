using System.Text;
using FluentAssertions;
using LegalDocsPro.Api.Controllers;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LegalDocsPro.Api.Tests.Controllers;

public class FilesControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly FilesController _controller;
    private readonly string _tempDir;

    public FilesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new FilesController(_mediatorMock.Object);

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
        var contract = new Contract("Title", "Description", "Client", documentUrl, null);
        contract.Id = id;
        contract.CreatedBy = createdBy;
        return contract;
    }
}
