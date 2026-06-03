using Microsoft.AspNetCore.Identity;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Services
{
    public interface IProjectService
    {
        Task<List<string>> HandleImageStoring(string userId, List<IFormFile> media, string folder, int maxMedia);

        Task HandleImageDeleting(List<string> media);
    }

    public class ProjectService: IProjectService
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;

        public ProjectService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this._env = env;
        }

        public async Task<List<string>> HandleImageStoring(string userId, List<IFormFile> media, string folder, int maxMedia)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var uploadedCount = 0;
            var maxMediaCount = maxMedia;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            List<string> mediaList = new List<string>();

            foreach (var file in media)
            {
                if (uploadedCount >= maxMediaCount)
                    break;

                if (file != null && file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        continue;
                    }

                    var fileName = $"{userId}_{DateTime.UtcNow.Ticks}_{Path.GetFileNameWithoutExtension(file.FileName)}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    mediaList.Add($"/uploads/{folder}/{fileName}");
                    uploadedCount++;
                }
            }
            return mediaList;
        }
    
        
        public async Task HandleImageDeleting(List<string> media)
        {
            foreach (var mediaPath in media)
            {
                if (!string.IsNullOrEmpty(mediaPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, mediaPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }
    
    }
}
