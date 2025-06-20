using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Queries.GetUserById;

public class GetUserByIdQueryHandler : IConsumer<GetUserByIdQuery>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public GetUserByIdQueryHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetUserByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
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