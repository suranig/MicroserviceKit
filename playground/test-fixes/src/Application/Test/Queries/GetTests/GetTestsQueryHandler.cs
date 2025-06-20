using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Queries.GetTests;

public class GetTestsQueryHandler : IConsumer<GetTestsQuery>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public GetTestsQueryHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetTestsQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
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