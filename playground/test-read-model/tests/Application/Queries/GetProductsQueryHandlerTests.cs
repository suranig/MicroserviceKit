using ReadModelService.Application.Product.Queries.GetProducts;
using ReadModelService.Application.Product.DTOs;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;

namespace ReadModelService.UnitTests.Application.Queries;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly GetProductsQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetProductsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _handler = new GetProductsQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Product>
        {
            new ProductBuilder().Build(),
            new ProductBuilder().Build(),
            new ProductBuilder().Build()
        };
        var query = new GetProductsQuery();

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
        var query = new GetProductsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}