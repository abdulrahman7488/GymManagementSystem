using GymManagementSystem.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public AttachmentService(IWebHostEnvironment env)
            => _env = env;

        public async Task<string?> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0) return null;

            // Validate extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return null;

            // Validate size
            if (file.Length > MaxFileSizeBytes) return null;

            // Build path: wwwroot/Attachments/{folder}/
            var uploadsFolder = Path.Combine(_env.WebRootPath, "Attachments", folder);
            Directory.CreateDirectory(uploadsFolder);

            // Unique file name to avoid collisions
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var fullPath   = Path.Combine(uploadsFolder, uniqueName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            // Return relative URL usable in <img src="...">
            return $"/Attachments/{folder}/{uniqueName}";
        }

        public Task<bool> DeleteAsync(string? relativeUrl, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return Task.FromResult(false);

            // Convert "/Attachments/Members/xyz.jpg" → full disk path
            var fullPath = Path.Combine(_env.WebRootPath, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath)) return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}
