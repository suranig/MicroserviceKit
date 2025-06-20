using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.RemoveItemOrder;

public class RemoveItemOrderCommandHandler : IConsumer<RemoveItemOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public RemoveItemOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<RemoveItemOrderCommand> context)
    {
        var command = context.Message;
        // TODO: Implement RemoveItem logic
        
        
    }
}