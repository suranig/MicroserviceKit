using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Queries.GetUsersWithPaging;

public class GetUsersWithPagingQueryHandler : IConsumer<GetUsersWithPagingQuery>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public GetUsersWithPagingQueryHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetUsersWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<UserDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
        await context.RespondAsync(result);
    }

    private UserDto MapToDto(SimpleService.Domain.Entities.User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}