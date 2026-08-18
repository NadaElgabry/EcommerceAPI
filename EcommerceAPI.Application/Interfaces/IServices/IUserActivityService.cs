using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(Guid userGuid, int? productId, UserActionType actionType, CancellationToken cancellationToken);
    }
}