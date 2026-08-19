using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;

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
    }
}