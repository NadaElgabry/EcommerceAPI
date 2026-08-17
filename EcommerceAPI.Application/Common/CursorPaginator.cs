/*using System.Linq.Expressions;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.Interfaces.Repositories;

namespace EcommerceAPI.Application.Common
{
    public static class CursorPaginator
    {
        public static async Task<PagedResult<TResponse>> PaginateAsync<
            TEntity,
            TCursor,
            TKey1,
            TKey2,
            TResponse>(
            IRepository<TEntity> repository,
            string? after,
            string? before,
            int pageSize,
            TCursor defaultCursor,
            Func<TCursor, Expression<Func<TEntity, bool>>> forwardPredicate,
            Func<TCursor, Expression<Func<TEntity, bool>>> backwardPredicate,
            Expression<Func<TEntity, TKey1>> orderBy,
            Expression<Func<TEntity, TKey2>> thenBy,
            Func<TEntity, TCursor> selectCursor,
            Func<TEntity, TResponse> map,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            if (!string.IsNullOrEmpty(after) &&
                !string.IsNullOrEmpty(before))
            {
                throw new Exceptions.BadRequestException(
                    "Both 'After' and 'Before' parameters cannot be provided at the same time.");
            }

            int fetchSize = pageSize + 1;

            bool navigatingBackward = !string.IsNullOrEmpty(before);

            List<TEntity> entities;
            bool hasMoreInQueryDirection;

            if (navigatingBackward)
            {
                TCursor cursor = CursorHelper.Decode<TCursor>(before!);

                entities = await repository.GetPagedDescendingAsync(
                    predicate: backwardPredicate(cursor),
                    orderBy: orderBy,
                    thenBy: thenBy,
                    take: fetchSize,
                    cancellationToken: cancellationToken);

                hasMoreInQueryDirection = entities.Count > pageSize;

                if (hasMoreInQueryDirection)
                {
                    entities = entities
                        .Take(pageSize)
                        .ToList();
                }

                // Put the page back into normal display order.
                entities.Reverse();
            }
            else
            {
                TCursor cursor;

                if (string.IsNullOrEmpty(after))
                {
                    cursor = defaultCursor;
                }
                else
                {
                    cursor = CursorHelper.Decode<TCursor>(after);
                }

                entities = await repository.GetPagedAsync(
                    predicate: forwardPredicate(cursor),
                    orderBy: orderBy,
                    thenBy: thenBy,
                    take: fetchSize,
                    cancellationToken: cancellationToken);

                hasMoreInQueryDirection = entities.Count > pageSize;

                if (hasMoreInQueryDirection)
                {
                    entities = entities
                        .Take(pageSize)
                        .ToList();
                }
            }

            string? startCursor = null;
            string? endCursor = null;

            if (entities.Count > 0)
            {
                startCursor = CursorHelper.Encode(
                    selectCursor(entities[0]));

                endCursor = CursorHelper.Encode(
                    selectCursor(entities[^1]));
            }

            bool hasNextPage =
                navigatingBackward
                    ? true
                    : hasMoreInQueryDirection;

            bool hasPreviousPage =
                navigatingBackward
                    ? hasMoreInQueryDirection
                    : !string.IsNullOrEmpty(after);

            return new PagedResult<TResponse>
            {
                Items = entities
                    .Select(map)
                    .ToList(),

                StartCursor = startCursor,
                EndCursor = endCursor,

                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }
    }
}*/