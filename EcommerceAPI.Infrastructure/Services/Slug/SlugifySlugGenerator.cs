using EcommerceAPI.Application.Interfaces.Slug;
using Microsoft.IdentityModel.Logging;
using Slugify;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Services.Slug
{
    public class SlugifySlugGenerator : ISlugGenerator
    {
        private readonly SlugHelper _slugHelper = new();

        public string GenerateSlug(string input) => _slugHelper.GenerateSlug(input);
    }
}
