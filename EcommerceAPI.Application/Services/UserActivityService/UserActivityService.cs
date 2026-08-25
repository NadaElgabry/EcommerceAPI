using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.UserActivities;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.Services.UserService
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IRepository<UserActivity> _activityRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserActivityService(IRepository<UserActivity> activityRepository, IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task LogActivityAsync(int userId, int? productId, UserActionType actionType, CancellationToken cancellationToken)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ProductId = productId,
                ActionType = actionType,
                Timestamp = DateTime.UtcNow
            };

            await _activityRepository.AddAsync(activity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<CursorPagedResult<UserActivitiesResponse>> GetAllActivitiesAsync(
           Guid? userId, string? cursor, int pageSize, CancellationToken cancellationToken)
        {
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            int? cursorId = string.IsNullOrWhiteSpace(cursor) ? null : CursorHelper.Decode<int>(cursor);
            User? user = null;
            if (userId.HasValue)
            {
                user = await _userRepository.GetByAsync(u => u.Guid == userId, cancellationToken)
                    ?? throw new NotFoundException("User not found.");
            }
            var activities = await _activityRepository.GetPagedDescendingAsync(
                predicate: a => (!userId.HasValue || a.UserId == user.Id) &&
                                (!cursorId.HasValue || a.Id < cursorId),
                include: q => q.Include(a => a.Product),
                orderBy: a => a.Id,
                take: pageSize + 1,
                cancellationToken: cancellationToken);

            bool hasNext = activities.Count > pageSize;
            if (hasNext) activities = activities.Take(pageSize).ToList();

            return new CursorPagedResult<UserActivitiesResponse>
            {
                Data = activities.Select(a => new UserActivitiesResponse
                {
                    UserId = user?.Guid ?? a.User?.Guid ?? Guid.Empty,
                    ActionType = a.ActionType.ToString(),
                    Slug = a.Product?.Slug,
                    Timestamp = a.Timestamp
                }).ToList(),
                Pagination = new CursorPageInfo
                {
                    NextCursor = hasNext ? CursorHelper.Encode(activities[^1].Id) : null,
                    HasNext = hasNext,
                    PageSize = pageSize
                }
            };
        }
    }
}