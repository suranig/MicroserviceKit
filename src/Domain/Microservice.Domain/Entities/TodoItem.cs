namespace Microservice.Domain.Entities;

public class TodoItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    public TodoItem(string title)
    {
        Title = title;
    }

    public void MarkComplete() => IsCompleted = true;
}
