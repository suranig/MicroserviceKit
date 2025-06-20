using SimpleService.Application.User.Queries.GetUserById;
using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using SimpleService.Domain.Entities;
using SimpleService.UnitTests.Builders;

namespace SimpleService.UnitTests.Application.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly GetUserByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUserByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _handler = new GetUserByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new UserBuilder().WithId(entityId).Build();
        var query = new GetUserByIdQuery(entityId);

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
        var query = new GetUserByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((User?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}