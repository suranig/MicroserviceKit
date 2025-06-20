using EventStoreService.Application.Event.Queries.GetEventsWithPaging;
using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;

namespace EventStoreService.UnitTests.Application.Queries;

public class GetEventsWithPagingQueryHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly GetEventsWithPagingQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetEventsWithPagingQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _handler = new GetEventsWithPagingQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidPaging_ShouldReturnPagedResult()
    {
        // Arrange
        var entities = Enumerable.Range(1, 25)
            .Select(_ => new EventBuilder().Build())
            .ToList();
        
        var pagedResult = new PagedResult<Event>
        {
            Items = entities.Take(10).ToList(),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        var query = new GetEventsWithPagingQuery(1, 10);

        _mockRepository.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task Handle_WithInvalidPaging_ShouldThrowArgumentException(int page, int pageSize)
    {
        // Arrange
        var query = new GetEventsWithPagingQuery(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}