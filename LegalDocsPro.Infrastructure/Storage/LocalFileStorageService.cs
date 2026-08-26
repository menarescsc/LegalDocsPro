using LegalDocsPro.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LegalDocsPro.Infrastructure.Storage
{
    /// <summary>
    /// Local-disk file storage implementation. Stores files outside wwwroot
    /// in a configurable private directory. Filenames are GUID-based to
    /// prevent path traversal attacks.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        private const string StorageSection = "FileStorage:BasePath";
        private const string DefaultSubfolder = "private-storage";

        public LocalFileStorageService(IConfiguration configuration)
        {
            var configuredPath = configuration[StorageSection];

            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                _basePath = configuredPath;
            }
            else
            {
                // Default: {ContentRoot}/private-storage — outside wwwroot, not served by static files
                var contentRoot = Directory.GetCurrentDirectory();
                _basePath = Path.Combine(contentRoot, DefaultSubfolder);
            }

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            var extension = Path.GetExtension(fileName);
            var storedName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(_basePath, storedName);

            // Guard against path traversal in the generated name
            var resolvedPath = Path.GetFullPath(fullPath);
            if (!resolvedPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid storage path detected.");

            await using var destination = new FileStream(resolvedPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await fileStream.CopyToAsync(destination, ct);

            return storedName;
        }

        public string? GetFilePath(string storedName)
        {
            if (string.IsNullOrWhiteSpace(storedName))
                return null;

            // Sanitize: only allow filename, no directory separators
            var safeName = Path.GetFileName(storedName);
            if (safeName != storedName)
                return null;

            var fullPath = Path.Combine(_basePath, safeName);

            // Double-check path stays within base directory
            var resolvedPath = Path.GetFullPath(fullPath);
            if (!resolvedPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
                return null;

            return File.Exists(resolvedPath) ? resolvedPath : null;
        }

        public Task DeleteAsync(string storedName, CancellationToken ct = default)
        {
            var filePath = GetFilePath(storedName);
            if (filePath != null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
