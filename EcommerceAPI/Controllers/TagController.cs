using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Tag;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
        {
            var result = await _tagService.CreateTagAsync(request, cancellationToken);
            return Created("api/tags", ApiResponse<TagResponse>.SuccessResponse(
                statusCode: 201, message: "Tag created successfully.", data: result));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTags([FromQuery] OffsetPageRequest request, CancellationToken cancellationToken)
        {
            var result = await _tagService.GetAllTagsAsync(request, cancellationToken);
            return Ok(ApiResponse<OffsetPagedResult<TagResponse>>.SuccessResponse(
                statusCode: 200, message: "Tags retrieved successfully.", data: result));
        }
        [HttpPut("{slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTag([FromRoute] string slug, [FromBody] UpdateTagRequest request, CancellationToken cancellationToken)
        {
            await _tagService.UpdateTagAsync(slug, request, cancellationToken);
            return Ok(ApiResponse<TagResponse>.SuccessResponse(
                statusCode: 200, message: "Tag updated successfully."));
        }

        [HttpDelete("{slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTagAsync(slug, cancellationToken);
            return StatusCode(
                204,
                ApiResponse<string>.SuccessResponse(message: "Product deleted successfully",
                statusCode: 204));
        }
    }
}