using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(int userId, int? productId, UserActionType actionType, CancellationToken cancellationToken);
    }
}