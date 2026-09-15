using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.DTOs.Rag;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.ExternalServices.Rag;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.Services.RagService
{
    public class RagService : IRagService
    {
        private readonly IRagClient _ragClient;
        private readonly ILogger<RagService> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserActivityService _userActivityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<UserActivity> _userActivityRepository;

        public RagService(
            IRagClient ragClient,
            IRepository<UserActivity> userActivityRepository,
            IRepository<User> userRepository,
            ILogger<RagService> logger,
            ICurrentUserService currentUserService,
            IUserActivityService userActivityService,
            IUnitOfWork unitOfWork)
        {
            _ragClient = ragClient;
            _userRepository = userRepository;
            _logger = logger;
            _currentUserService = currentUserService;
            _userActivityService = userActivityService;
            _unitOfWork = unitOfWork;
            _userActivityRepository = userActivityRepository;
        }

        public async Task<AnswerResponse> AskAsync(string question, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found");
            var request = new QuestionRequest { Question = question, UserId = user.Id.ToString() };
            return await _ragClient.AskAsync(request, cancellationToken);
        }

        public async Task<bool> TerminateAsync(CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found");
            var raw = await _ragClient.TerminateAsync(user.Id.ToString(), cancellationToken);

            List<UserActivity> userActivities = new List<UserActivity>();
            foreach (var id in raw.SuggestedProducts.Distinct())
            {
                userActivities.Add(_userActivityService.BuildActivity(user.Id,id,Domain.Enums.UserActionType.Chatbot));
            }
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userActivityRepository.AddRangeAsync(userActivities, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            }, cancellationToken);
            return true; 
        }
    }
}