using Company.TestService.Application.Item.Queries.GetItemById;
using Company.TestService.Application.Item.DTOs;
using Company.TestService.Application.Common;
using Company.TestService.Domain.Entities;
using Company.TestService.UnitTests.Builders;

namespace Company.TestService.UnitTests.Application.Queries;

public class GetItemByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;
    private readonly GetItemByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetItemByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
        _handler = new GetItemByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new ItemBuilder().WithId(entityId).Build();
        var query = new GetItemByIdQuery(entityId);

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
        var query = new GetItemByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Item?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}