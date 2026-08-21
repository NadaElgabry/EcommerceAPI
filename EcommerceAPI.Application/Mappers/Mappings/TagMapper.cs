using EcommerceAPI.Application.DTOs.Tag;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class TagMapper : ITagMapper
    {
        public Tag toTag(CreateTagRequest request, string slug)
        {
            return new Tag { Name = request.Name, Slug = slug };
        }

        public TagResponse toTagResponse(Tag tag)
        {
            return new TagResponse { Name = tag.Name, Slug = tag.Slug };
        }

        public void updateTagFromRequest(Tag tag, UpdateTagRequest request, string slug)
        {
            tag.Name = request.Name;
            tag.Slug = slug;
        }
    }
}
