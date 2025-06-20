using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Queries.GetUsers;

public class GetUsersQueryHandler : IConsumer<GetUsersQuery>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public GetUsersQueryHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetUsersQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
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