using SimpleService.Application.User.Queries.GetUsers;
using SimpleService.Application.User.DTOs;
using SimpleService.Application.Common;
using SimpleService.Domain.Entities;
using SimpleService.UnitTests.Builders;

namespace SimpleService.UnitTests.Application.Queries;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly GetUsersQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUsersQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _handler = new GetUsersQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<User>
        {
            new UserBuilder().Build(),
            new UserBuilder().Build(),
            new UserBuilder().Build()
        };
        var query = new GetUsersQuery();

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
        var query = new GetUsersQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<User>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}