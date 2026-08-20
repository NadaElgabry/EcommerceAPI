using EcommerceAPI.Application.DTOs.Tag;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface ITagMapper
    {
        Tag toTag(CreateTagRequest request, string slug);
        TagResponse toTagResponse(Tag tag);
        void updateTagFromRequest(Tag tag, UpdateTagRequest request, string slug);
    }
}
