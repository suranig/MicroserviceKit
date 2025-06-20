using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Queries.GetTestsWithPaging;

public class GetTestsWithPagingQueryHandler : IConsumer<GetTestsWithPagingQuery>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public GetTestsWithPagingQueryHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetTestsWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<TestDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
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