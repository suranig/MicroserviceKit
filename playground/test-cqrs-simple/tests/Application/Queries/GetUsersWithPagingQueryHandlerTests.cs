using SimpleService.Application.User.Queries.GetUsersWithPaging;
using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using SimpleService.Domain.Entities;
using SimpleService.UnitTests.Builders;

namespace SimpleService.UnitTests.Application.Queries;

public class GetUsersWithPagingQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly GetUsersWithPagingQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUsersWithPagingQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _handler = new GetUsersWithPagingQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidPaging_ShouldReturnPagedResult()
    {
        // Arrange
        var entities = Enumerable.Range(1, 25)
            .Select(_ => new UserBuilder().Build())
            .ToList();
        
        var pagedResult = new PagedResult<User>
        {
            Items = entities.Take(10).ToList(),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        var query = new GetUsersWithPagingQuery(1, 10);

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
        var query = new GetUsersWithPagingQuery(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}