using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Slug
{
    public interface ISlugGenerator
    {
        string GenerateSlug(string input);
    }
}
