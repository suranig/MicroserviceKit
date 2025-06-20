using EventStoreService.Application.Event.Queries.GetEventById;
using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;

namespace EventStoreService.UnitTests.Application.Queries;

public class GetEventByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly GetEventByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetEventByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _handler = new GetEventByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new EventBuilder().WithId(entityId).Build();
        var query = new GetEventByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entity);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entityId);
    }

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldReturnNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var query = new GetEventByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Event?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}