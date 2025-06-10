using Company.TestService.Application.Item.Commands.MarkCompleteItem;
using Company.TestService.Application.Common;
using Company.TestService.Domain.Entities;
using Company.TestService.UnitTests.Builders;
using Company.TestService.UnitTests.Utilities;

namespace Company.TestService.UnitTests.Application.Commands;

public class MarkCompleteItemCommandHandlerTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;
    private readonly Mock<ILogger<MarkCompleteItemCommandHandler>> _mockLogger;
    private readonly MarkCompleteItemCommandHandler _handler;
    private readonly Fixture _fixture;

    public MarkCompleteItemCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
        _mockLogger = new Mock<ILogger<MarkCompleteItemCommandHandler>>();
        _handler = new MarkCompleteItemCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteMarkCompleteSuccessfully()
    {
        // Arrange
        var command = new MarkCompleteItemCommand("Test Value", true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for MarkComplete operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}