using EcommerceAPI.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Image
{
    public interface IImageService
    {
        public Task<string> SaveFileAsync(IFormFile imageFile, string fileNameHint, ImageOwnerType ownerType, CancellationToken cancellationToken = default);
        public void DeleteFile(string fileNameWithExtension, ImageOwnerType ownerType);
    }
}
