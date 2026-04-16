using Microsoft.AspNetCore.Http;

namespace AIInterview.Server.Services
{
    public interface ICloudinaryFileService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, string publicIdPrefix, CancellationToken cancellationToken = default);
        Task<CloudinaryUploadResult> UploadRawAsync(IFormFile file, string folder, string publicIdPrefix, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string? fileUrl, CancellationToken cancellationToken = default);
    }

    public sealed class CloudinaryUploadResult
    {
        public required string Url { get; init; }
        public required string PublicId { get; init; }
        public required string ResourceType { get; init; }
        public string? OriginalFileName { get; init; }
    }
}
