using TestService.Application.Product.Commands.CreateProduct;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _mockLogger;
    private readonly CreateProductCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
        _handler = new CreateProductCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateProductCommand(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateProductCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Product>(e => 
e.Id == command.Id &&
            e.CreatedAt == command.CreatedAt &&
            e.UpdatedAt == command.UpdatedAt
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateProductCommand(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}