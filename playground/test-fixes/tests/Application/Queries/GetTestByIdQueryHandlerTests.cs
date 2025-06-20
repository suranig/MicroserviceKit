using TestService.Application.Test.Queries.GetTestById;
using TestService.Application.Test.DTOs;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Application.Queries;

public class GetTestByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly GetTestByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetTestByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _handler = new GetTestByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestBuilder().WithId(entityId).Build();
        var query = new GetTestByIdQuery(entityId);

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
        var query = new GetTestByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Test?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}