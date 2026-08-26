using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Tag;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly IRepository<Tag> _tagRepository;
        private readonly ITagMapper _tagMapper;
        private readonly ISlugGenerator _slugGenerator;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IProductIndexingService _indexingService;

        private readonly IRepository<Product> _productRepository;

        private readonly ILogger<TagService> _logger;

        public TagService(IRepository<Tag> tagRepository, ITagMapper tagMapper,
            ISlugGenerator slugGenerator, IUnitOfWork unitOfWork,
            IProductIndexingService indexingService, IRepository<Product> productRepository,
            ILogger<TagService> logger)
        {
            _tagRepository = tagRepository;
            _tagMapper = tagMapper;
            _slugGenerator = slugGenerator;
            _unitOfWork = unitOfWork;
            _indexingService = indexingService;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<TagResponse> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken)
        {
            var slug = _slugGenerator.GenerateSlug(request.Name);

            if (await _tagRepository.ExistByAsync(t => t.Slug == slug, cancellationToken))
            {
                throw new ConflictException("A tag with this name already exists.");
            }


            var tag = _tagMapper.toTag(request, slug);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _tagRepository.AddAsync(tag, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return _tagMapper.toTagResponse(tag);
        }

        public async Task<OffsetPagedResult<TagResponse>> GetAllTagsAsync(
OffsetPageRequest request,
CancellationToken cancellationToken = default)
        {
            int page = Math.Max(request.PageNumber, 1);
            int pageSize = Math.Clamp(request.PageSize, 1, 100);

            var tags = await _tagRepository.GetPageOffSetAsync(
                orderBy: t => t.Id,
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            var totalCount = await _tagRepository.GetCountAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new OffsetPagedResult<TagResponse>
            {
                Data = tags.Select(t => _tagMapper.toTagResponse(t)).ToList(),
                Pagination = new PageInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }
        public async Task UpdateTagAsync(string newSlug, UpdateTagRequest request, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetByAsync(t => t.Slug == newSlug, cancellationToken)
                ?? throw new NotFoundException($"Tag with Slug: {newSlug} not found.");
            if (await _tagRepository.ExistByAsync(t => t.Name == request.Name && t.Slug != newSlug, cancellationToken))
            {
                throw new ConflictException("A tag with this name already exists.");
            }
            var slug = _slugGenerator.GenerateSlug(request.Name);
            _tagMapper.updateTagFromRequest(tag, request, slug);
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _tagRepository.Update(tag);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
            var affectedProducts = await _productRepository.GetAllAsync(
             predicate: p => p.ProductTags.Any(pt => pt.TagId == tag.Id),
             include: q => q.Include(p => p.Category).Include(p => p.ProductTags).ThenInclude(pt => pt.Tag),
             cancellationToken: cancellationToken);

            foreach (var product in affectedProducts)
            {
                try
                {
                    await _indexingService.IndexProductAsync(product, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reindex product {ProductId} after tag rename.", product.Id);
                }
            }
        }

        public async Task DeleteTagAsync(string slug, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetByAsync(t => t.Slug == slug, cancellationToken)
                ?? throw new NotFoundException($"Tag with slug: {slug} not found.");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _tagRepository.Delete(tag);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }


    }
}
