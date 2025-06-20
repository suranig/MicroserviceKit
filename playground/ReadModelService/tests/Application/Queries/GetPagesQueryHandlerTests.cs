using ReadModelService.Application.Page.Queries.GetPages;
using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;

namespace ReadModelService.UnitTests.Application.Queries;

public class GetPagesQueryHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly GetPagesQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetPagesQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _handler = new GetPagesQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Page>
        {
            new PageBuilder().Build(),
            new PageBuilder().Build(),
            new PageBuilder().Build()
        };
        var query = new GetPagesQuery();

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
        var query = new GetPagesQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Page>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}