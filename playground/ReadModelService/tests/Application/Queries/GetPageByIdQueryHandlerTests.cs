using ReadModelService.Application.Page.Queries.GetPageById;
using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;

namespace ReadModelService.UnitTests.Application.Queries;

public class GetPageByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly GetPageByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetPageByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _handler = new GetPageByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new PageBuilder().WithId(entityId).Build();
        var query = new GetPageByIdQuery(entityId);

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
        var query = new GetPageByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Page?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}