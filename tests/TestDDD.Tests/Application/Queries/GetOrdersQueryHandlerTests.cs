using ECommerce.OrderService.Application.Order.Queries.GetOrders;
using ECommerce.OrderService.Application.Order.DTOs;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;

namespace ECommerce.OrderService.UnitTests.Application.Queries;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly GetOrdersQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetOrdersQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _handler = new GetOrdersQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Order>
        {
            new OrderBuilder().Build(),
            new OrderBuilder().Build(),
            new OrderBuilder().Build()
        };
        var query = new GetOrdersQuery();

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
        var query = new GetOrdersQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Order>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}