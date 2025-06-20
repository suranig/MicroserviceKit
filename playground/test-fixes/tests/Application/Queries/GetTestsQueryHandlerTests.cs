using TestService.Application.Test.Queries.GetTests;
using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Application.Queries;

public class GetTestsQueryHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly GetTestsQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetTestsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _handler = new GetTestsQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Test>
        {
            new TestBuilder().Build(),
            new TestBuilder().Build(),
            new TestBuilder().Build()
        };
        var query = new GetTestsQuery();

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
        var query = new GetTestsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Test>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}