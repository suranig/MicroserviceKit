using ECommerce.OrderService.Application.Customer.Queries.GetCustomers;
using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;

namespace ECommerce.OrderService.UnitTests.Application.Queries;

public class GetCustomersQueryHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly GetCustomersQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetCustomersQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _handler = new GetCustomersQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Customer>
        {
            new CustomerBuilder().Build(),
            new CustomerBuilder().Build(),
            new CustomerBuilder().Build()
        };
        var query = new GetCustomersQuery();

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
        var query = new GetCustomersQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}