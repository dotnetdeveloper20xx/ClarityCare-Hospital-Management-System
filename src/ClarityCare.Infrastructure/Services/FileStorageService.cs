using ClarityCare.Application.Common.Interfaces;

namespace ClarityCare.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Path.Combine(AppContext.BaseDirectory, "Storage", "Documents");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueName);
        using var outputStream = File.Create(filePath);
        await fileStream.CopyToAsync(outputStream, cancellationToken);
        return uniqueName;
    }

    public Task<Stream> GetFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", storagePath);
        Stream stream = File.OpenRead(filePath);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}
