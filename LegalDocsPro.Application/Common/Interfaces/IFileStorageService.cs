namespace LegalDocsPro.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for file storage operations. Keeps the controller storage-agnostic
    /// for future migration to blob storage (e.g., Azure Blob, S3).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves a file stream to storage with a GUID-based filename.
        /// Returns the stored name (e.g., "{guid}.pdf").
        /// </summary>
        Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default);

        /// <summary>
        /// Resolves the full filesystem path for a stored name.
        /// Returns null if the file does not exist.
        /// </summary>
        string? GetFilePath(string storedName);

        /// <summary>
        /// Deletes a file from storage by its stored name.
        /// </summary>
        Task DeleteAsync(string storedName, CancellationToken ct = default);
    }
}
