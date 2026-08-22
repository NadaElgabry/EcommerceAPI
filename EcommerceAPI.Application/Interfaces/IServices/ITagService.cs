using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Tag;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface ITagService
    {
        Task<TagResponse> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken);
        public Task<OffsetPagedResult<TagResponse>> GetAllTagsAsync(OffsetPageRequest request,CancellationToken cancellationToken = default);        
        Task UpdateTagAsync(string slug, UpdateTagRequest request, CancellationToken cancellationToken);
        Task DeleteTagAsync(string slug, CancellationToken cancellationToken);
    }

}
