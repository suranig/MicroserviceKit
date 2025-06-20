using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler : IConsumer<CreateOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public CreateOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        var command = context.Message;
        var entity = new TestService.Domain.Entities.Order(command.CustomerId, command.TotalAmount, command.Status);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}