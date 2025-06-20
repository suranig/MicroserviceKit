using TestService.Application.Product.Queries.GetProductById;
using TestService.Application.Product.DTOs;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Application.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly GetProductByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetProductByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _handler = new GetProductByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new ProductBuilder().WithId(entityId).Build();
        var query = new GetProductByIdQuery(entityId);

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
        var query = new GetProductByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Product?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}