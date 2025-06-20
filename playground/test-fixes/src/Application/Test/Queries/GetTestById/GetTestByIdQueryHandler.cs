using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Queries.GetTestById;

public class GetTestByIdQueryHandler : IConsumer<GetTestByIdQuery>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public GetTestByIdQueryHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetTestByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
        await context.RespondAsync(result);
    }

    private TestDto MapToDto(TestService.Domain.Entities.Test entity)
    {
        return new TestDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}