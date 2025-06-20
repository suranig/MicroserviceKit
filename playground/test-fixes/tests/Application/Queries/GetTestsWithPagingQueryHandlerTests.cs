using TestService.Application.Test.Queries.GetTestsWithPaging;
using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Application.Queries;

public class GetTestsWithPagingQueryHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly GetTestsWithPagingQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetTestsWithPagingQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _handler = new GetTestsWithPagingQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidPaging_ShouldReturnPagedResult()
    {
        // Arrange
        var entities = Enumerable.Range(1, 25)
            .Select(_ => new TestBuilder().Build())
            .ToList();
        
        var pagedResult = new PagedResult<Test>
        {
            Items = entities.Take(10).ToList(),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        var query = new GetTestsWithPagingQuery(1, 10);

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
        var query = new GetTestsWithPagingQuery(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}