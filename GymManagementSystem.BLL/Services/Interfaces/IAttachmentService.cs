using Microsoft.AspNetCore.Http;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IAttachmentService
    {
        /// <summary>Saves file to wwwroot/Attachments/{folder} and returns the relative URL.</summary>
        Task<string?> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);

        /// <summary>Deletes a file given its relative URL (e.g. "/Attachments/Members/xyz.jpg").</summary>
        Task<bool> DeleteAsync(string? relativeUrl, CancellationToken ct = default);
    }
}
