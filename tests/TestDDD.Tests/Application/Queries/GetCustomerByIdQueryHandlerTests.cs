using ECommerce.OrderService.Application.Customer.Queries.GetCustomerById;
using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;

namespace ECommerce.OrderService.UnitTests.Application.Queries;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly GetCustomerByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetCustomerByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _handler = new GetCustomerByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new CustomerBuilder().WithId(entityId).Build();
        var query = new GetCustomerByIdQuery(entityId);

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
        var query = new GetCustomerByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Customer?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}