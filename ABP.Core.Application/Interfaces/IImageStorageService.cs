using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface IImageStorageService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string id, string folderName, bool isEditMode = false, string? existingImagePath = "");
        bool DeleteImage(string id, string folderName);
    }
}
