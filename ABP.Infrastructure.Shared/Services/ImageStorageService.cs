using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Helpers.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;


namespace ABP.Infrastructure.Shared.Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string id, string folderName, bool isEditMode = false, string? existingImagePath = "")
        {
            if (isEditMode && file == null)
            {
                return existingImagePath;
            }

            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            if (!ImageValidator.ValidateFile(file, out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            string basePath = $"Images/{folderName}/{id}";
            string path = Path.Combine(_env.WebRootPath, basePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Guid guid = Guid.NewGuid();
            FileInfo fileInfo = new(file.FileName);
            string fileName = guid + fileInfo.Extension;

            string fullFilePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (isEditMode && !string.IsNullOrWhiteSpace(existingImagePath))
            {
                string[] oldImagePart = existingImagePath.Split("/");
                string oldFileName = oldImagePart[^1];
                string completeOldPath = Path.Combine(path, oldFileName);

                if (File.Exists(completeOldPath))
                {
                    File.Delete(completeOldPath);
                }
            }

            return $"/{basePath}/{fileName}";
        }

        public bool DeleteImage(string id, string folderName)
        {
            string basePath = $"Images/{folderName}/{id}";
            string path = Path.Combine(_env.WebRootPath, basePath);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return true;
            }

            return false;
        }
    }
}
