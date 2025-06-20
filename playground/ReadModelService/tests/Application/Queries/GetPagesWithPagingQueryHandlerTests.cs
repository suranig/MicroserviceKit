using ReadModelService.Application.Page.Queries.GetPagesWithPaging;
using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;

namespace ReadModelService.UnitTests.Application.Queries;

public class GetPagesWithPagingQueryHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly GetPagesWithPagingQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetPagesWithPagingQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _handler = new GetPagesWithPagingQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidPaging_ShouldReturnPagedResult()
    {
        // Arrange
        var entities = Enumerable.Range(1, 25)
            .Select(_ => new PageBuilder().Build())
            .ToList();
        
        var pagedResult = new PagedResult<Page>
        {
            Items = entities.Take(10).ToList(),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        var query = new GetPagesWithPagingQuery(1, 10);

        _mockRepository.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task Handle_WithInvalidPaging_ShouldThrowArgumentException(int page, int pageSize)
    {
        // Arrange
        var query = new GetPagesWithPagingQuery(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}