using OrderService.Domain.Entities;
using OrderService.Application.Common;
using MassTransit;

namespace OrderService.Application.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler : IConsumer<CreateOrderCommand>
{
    private readonly IRepository<OrderService.Domain.Entities.Order> _repository;

    public CreateOrderCommandHandler(IRepository<OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        var command = context.Message;
        var entity = new OrderService.Domain.Entities.Order(command.Name, command.Description);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}