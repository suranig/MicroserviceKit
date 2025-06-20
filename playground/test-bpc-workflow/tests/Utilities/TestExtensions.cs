using System.Reflection;

namespace WorkflowService.UnitTests.Utilities;

public static class TestExtensions
{
    public static T SetPrivateProperty<T>(this T obj, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (property != null)
        {
            property.SetValue(obj, value);
        }
        return obj;
    }

    public static object? GetPrivateProperty<T>(this T obj, string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(obj);
    }

    public static T SetPrivateField<T>(this T obj, string fieldName, object value)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        return obj;
    }

    public static object? GetPrivateField<T>(this T obj, string fieldName)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(obj);
    }

    public static void ShouldRaiseDomainEvent<TEvent>(this AggregateKit.AggregateRoot<Guid> aggregate)
        where TEvent : AggregateKit.DomainEventBase
    {
        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TEvent>();
    }

    public static void ShouldRaiseDomainEvents<TEvent>(this AggregateKit.AggregateRoot<Guid> aggregate, int expectedCount)
        where TEvent : AggregateKit.DomainEventBase
    {
        aggregate.DomainEvents.OfType<TEvent>().Should().HaveCount(expectedCount);
    }
}