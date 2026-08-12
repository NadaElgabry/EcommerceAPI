using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Image
{
    public interface IImageService
    {
        public Task<string> SaveFileAsync(IFormFile imageFile);
        public void DeleteFile(string fileNameWithExtension);
    }
}
