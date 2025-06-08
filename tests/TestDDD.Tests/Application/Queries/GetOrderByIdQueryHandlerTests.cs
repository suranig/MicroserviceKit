using ECommerce.OrderService.Application.Order.Queries.GetOrderById;
using ECommerce.OrderService.Application.Order.DTOs;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;

namespace ECommerce.OrderService.UnitTests.Application.Queries;

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