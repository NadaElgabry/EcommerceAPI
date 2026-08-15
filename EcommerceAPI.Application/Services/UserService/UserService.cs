using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Mappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.Interfaces.Repositories;

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

        public async Task<PagedResult<UserResponse>> GetUsersAsync(
    GetUsersRequest request, CancellationToken cancellationToken = default)
        {
            if(!string.IsNullOrEmpty(request.After) && !string.IsNullOrEmpty(request.Before))
            {
                throw new BadRequestException("Both 'After' and 'Before' parameters cannot be provided at the same time.");
            }

            int fetchSize= request.PageSize + 1; // Fetch one extra to determine if there's a next page

            List<User> users;

            bool hasMoreInQueryDirection;
            bool navigatingBackward = !string.IsNullOrEmpty(request.Before);

            if (navigatingBackward)
            {
                var cursor = CursorHelper.Decode<UserCursor>(request.Before!);

                users = await _userRepository.GetPagedDescendingAsync(
                    predicate: u => u.CreatedAt < cursor.CreatedAt || (u.CreatedAt == cursor.CreatedAt && u.Id < cursor.Id),
                    orderBy: u => u.CreatedAt,
                    thenBy: u => u.Id,
                    take: fetchSize,
                    cancellationToken: cancellationToken
                );

                hasMoreInQueryDirection = users.Count > request.PageSize;
                if (hasMoreInQueryDirection)
                {
                    users = users.Take(request.PageSize).ToList();
                }
                users.Reverse();
            }
            else
            {
                var cursor = string.IsNullOrEmpty(request.After)
                    ? new UserCursor(DateTime.MinValue,int.MinValue)
                    :CursorHelper.Decode<UserCursor>(request.After);

                users = await _userRepository.GetPagedAsync(
                    predicate: u => u.CreatedAt > cursor.CreatedAt || (u.CreatedAt == cursor.CreatedAt && u.Id > cursor.Id),
                    orderBy: u => u.CreatedAt,
                    thenBy: u => u.Id,
                    take: fetchSize,
                    cancellationToken: cancellationToken
                );
                hasMoreInQueryDirection = users.Count > request.PageSize;
                if(hasMoreInQueryDirection) {
                    users = users.Take(request.PageSize).ToList();
                }
            }
            string? startCursor = null;
            string? endCursor = null;
            if (users.Count > 0)
            {
                startCursor = CursorHelper.Encode(new UserCursor(users[0].CreatedAt, users[0].Id));
                endCursor=CursorHelper.Encode(new UserCursor(users[^1].CreatedAt, users[^1].Id));
            }

            bool hasNextPage = navigatingBackward ? true : hasMoreInQueryDirection;
            bool hasPreviousPage = navigatingBackward ? hasMoreInQueryDirection : !string.IsNullOrEmpty(request.After);

            return new PagedResult<UserResponse>
            {
                Items = users.Select(_userMapper.ToUserResponse).ToList(),
                StartCursor = startCursor,
                EndCursor = endCursor,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage,
            };
        }
    }
}
