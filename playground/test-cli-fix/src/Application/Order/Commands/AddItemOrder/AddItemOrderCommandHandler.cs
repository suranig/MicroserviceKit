using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.AddItemOrder;

public class AddItemOrderCommandHandler : IConsumer<AddItemOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public AddItemOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<AddItemOrderCommand> context)
    {
        var command = context.Message;
        // TODO: Implement AddItem logic
        
        
    }
}