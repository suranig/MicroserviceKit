using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.CancelOrder;

public class CancelOrderCommandHandler : IConsumer<CancelOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public CancelOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var command = context.Message;
        // TODO: Implement Cancel logic
        
        
    }
}