using OrderService.Application.Order.Commands.CreateOrder;
using OrderService.Application.Common;
using OrderService.Domain.Entities;
using OrderService.UnitTests.Builders;
using OrderService.UnitTests.Utilities;

namespace OrderService.UnitTests.Application.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private readonly CreateOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
        _handler = new CreateOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), "Test Value", "Test Value");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateOrderCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Order>(e => 
e.Id == command.Id &&
            e.Name == command.Name &&
            e.Description == command.Description
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), "Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}