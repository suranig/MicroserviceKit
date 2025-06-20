using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandler : IConsumer<ConfirmOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public ConfirmOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<ConfirmOrderCommand> context)
    {
        var command = context.Message;
        // TODO: Implement Confirm logic
        
        
    }
}