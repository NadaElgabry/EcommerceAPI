using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.UserActivities;
using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(int userId, int? productId, UserActionType actionType, CancellationToken cancellationToken);
        Task<CursorPagedResult<UserActivitiesResponse>> GetAllActivitiesAsync(Guid? userId, string? cursor, int pageSize, CancellationToken cancellationToken);
    }
}