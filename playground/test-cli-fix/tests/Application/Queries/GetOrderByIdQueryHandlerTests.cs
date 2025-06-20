using TestService.Application.Order.Queries.GetOrderById;
using TestService.Application.Order.DTOs;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Application.Queries;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly GetOrderByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetOrderByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _handler = new GetOrderByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new OrderBuilder().WithId(entityId).Build();
        var query = new GetOrderByIdQuery(entityId);

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
        var query = new GetOrderByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Order?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}