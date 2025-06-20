using ReadModelService.Application.Product.Commands.CreateProduct;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;
using ReadModelService.UnitTests.Utilities;

namespace ReadModelService.UnitTests.Application.Commands;

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
        var command = new CreateProductCommand(Guid.NewGuid(), "Test Value", "Test Value");

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
            e.Name == command.Name &&
            e.Description == command.Description
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateProductCommand(Guid.NewGuid(), "Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}