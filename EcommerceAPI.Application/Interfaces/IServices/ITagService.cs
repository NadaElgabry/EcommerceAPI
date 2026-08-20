using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Tag;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface ITagService
    {
        Task<TagResponse> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken);
        public Task<OffsetPagedResult<TagResponse>> GetAllTagsAsync(GetTagsRequest request,CancellationToken cancellationToken = default);        
        Task UpdateTagAsync(int id, UpdateTagRequest request, CancellationToken cancellationToken);
        Task DeleteTagAsync(int id, CancellationToken cancellationToken);
    }

}
