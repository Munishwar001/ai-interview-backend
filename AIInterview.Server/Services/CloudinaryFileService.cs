using AIInterview.Server.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace AIInterview.Server.Services
{
    public class CloudinaryFileService : ICloudinaryFileService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.CloudName)
                || string.IsNullOrWhiteSpace(settings.ApiKey)
                || string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException("CloudinarySettings is missing required values.");
            }

            Console.WriteLine("cloud name =>", settings.CloudName);
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }

        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, string publicIdPrefix, CancellationToken cancellationToken = default)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = BuildPublicId(publicIdPrefix),
                UseFilename = false,
                UniqueFilename = false,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            ValidateUploadResult(result.Error, result.SecureUrl?.ToString());

            return new CloudinaryUploadResult
            {
                Url = result.SecureUrl!.ToString(),
                PublicId = result.PublicId,
                ResourceType = "image",
                OriginalFileName = file.FileName
            };
        }

        public async Task<CloudinaryUploadResult> UploadRawAsync(IFormFile file, string folder, string publicIdPrefix, CancellationToken cancellationToken = default)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Type = "upload",
                AccessMode = "public",
                PublicId = BuildPublicId(publicIdPrefix),
                UseFilename = false,
                UniqueFilename = false,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
            ValidateUploadResult(result.Error, result.SecureUrl?.ToString());

            return new CloudinaryUploadResult
            {
                Url = result.SecureUrl!.ToString(),
                PublicId = result.PublicId,
                ResourceType = "raw",
                OriginalFileName = file.FileName
            };
        }

        public async Task<bool> DeleteAsync(string? fileUrl, CancellationToken cancellationToken = default)
        {
            if (!TryParseCloudinaryAsset(fileUrl, out var publicId, out var resourceType))
                return false;

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result is "ok" or "not found";
        }

        private static string BuildPublicId(string prefix)
            => $"{prefix}_{Guid.NewGuid():N}";

        private static void ValidateUploadResult(Error? error, string? secureUrl)
        {
            if (error != null)
                throw new InvalidOperationException($"Cloudinary upload failed: {error.Message}");

            if (string.IsNullOrWhiteSpace(secureUrl))
                throw new InvalidOperationException("Cloudinary upload failed: no secure URL returned.");
        }

        private static bool TryParseCloudinaryAsset(string? fileUrl, out string publicId, out ResourceType resourceType)
        {
            publicId = string.Empty;
            resourceType = ResourceType.Image;

            if (string.IsNullOrWhiteSpace(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                return false;

            if (!uri.Host.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
                return false;

            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var uploadIndex = Array.FindIndex(segments, s => s.Equals("upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 2 || uploadIndex >= segments.Length - 1)
                return false;

            resourceType = segments[uploadIndex - 1].ToLowerInvariant() switch
            {
                "raw" => ResourceType.Raw,
                "video" => ResourceType.Video,
                _ => ResourceType.Image
            };

            var publicIdStart = uploadIndex + 1;
            if (segments[publicIdStart].StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && segments[publicIdStart].Length > 1
                && int.TryParse(segments[publicIdStart][1..], out _))
            {
                publicIdStart++;
            }

            if (publicIdStart >= segments.Length)
                return false;

            var publicIdParts = segments[publicIdStart..].ToArray();
            if (resourceType != ResourceType.Raw)
            {
                var last = publicIdParts[^1];
                var extensionIndex = last.LastIndexOf('.');
                if (extensionIndex > 0)
                    publicIdParts[^1] = last[..extensionIndex];
            }

            publicId = string.Join('/', publicIdParts);
            return !string.IsNullOrWhiteSpace(publicId);
        }
    }
}
