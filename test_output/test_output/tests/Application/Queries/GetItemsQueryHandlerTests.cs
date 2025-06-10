using Company.TestService.Application.Item.Queries.GetItems;
using Company.TestService.Application.Item.DTOs;
using Company.TestService.Application.Common;
using Company.TestService.Domain.Entities;
using Company.TestService.UnitTests.Builders;

namespace Company.TestService.UnitTests.Application.Queries;

public class GetItemsQueryHandlerTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;
    private readonly GetItemsQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetItemsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
        _handler = new GetItemsQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Item>
        {
            new ItemBuilder().Build(),
            new ItemBuilder().Build(),
            new ItemBuilder().Build()
        };
        var query = new GetItemsQuery();

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
        var query = new GetItemsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Item>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}