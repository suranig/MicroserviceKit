using EventStoreService.Application.Event.Queries.GetEvents;
using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;

namespace EventStoreService.UnitTests.Application.Queries;

public class GetEventsQueryHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly GetEventsQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetEventsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _handler = new GetEventsQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Event>
        {
            new EventBuilder().Build(),
            new EventBuilder().Build(),
            new EventBuilder().Build()
        };
        var query = new GetEventsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Handle_WithNoEntities_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetEventsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Event>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}