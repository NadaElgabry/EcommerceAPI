using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Mappers.Interfaces;

using EcommerceAPI.Application.Common;

namespace EcommerceAPI.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserMapper _userMapper;

        private record UserCursor(DateTime CreatedAt, int Id);
        
        public UserService(IRepository<User> userRepository, IUserMapper userMapper)
        {
            _userRepository = userRepository;
            _userMapper = userMapper;
        }

        public Task<PagedResult<UserResponse>> GetAllUsersAsync(
            GetUsersRequest request,
            CancellationToken cancellationToken = default)
        {
            return CursorPaginator.PaginateAsync(
                repository: _userRepository,

                after: request.After,
                before: request.Before,

                pageSize: request.PageSize,

                defaultCursor: new UserCursor(
                    DateTime.MinValue,
                    int.MinValue),

                forwardPredicate: c =>
                    u =>
                        u.CreatedAt > c.CreatedAt ||
                        (u.CreatedAt == c.CreatedAt &&
                         u.Id > c.Id),

                backwardPredicate: c =>
                    u =>
                        u.CreatedAt < c.CreatedAt ||
                        (u.CreatedAt == c.CreatedAt &&
                         u.Id < c.Id),

                orderBy: u => u.CreatedAt,
                thenBy: u => u.Id,

                selectCursor: u =>
                    new UserCursor(
                        u.CreatedAt,
                        u.Id),

                map: _userMapper.ToUserResponse,

                cancellationToken: cancellationToken);
        }
    }
}
