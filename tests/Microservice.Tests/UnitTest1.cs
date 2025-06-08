using Microservice.Application.Extensions;
using Microservice.Application.Todo.Commands.CreateTodo;
using Microservice.Application.Todo.Queries.GetTodos;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Microservice.Tests;

public class TodoTests
{
    [Fact]
    public async Task CreatingTodoReturnsId()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();
        using var provider = services.BuildServiceProvider();

        var messageBus = provider.GetRequiredService<IMessageBus>();
        var id = await messageBus.InvokeAsync<Guid>(new CreateTodoCommand("test"));
        var todos = await messageBus.InvokeAsync<IReadOnlyList<TodoItem>>(new GetTodosQuery());
        Assert.Contains(todos, x => x.Id == id);
    }
}
