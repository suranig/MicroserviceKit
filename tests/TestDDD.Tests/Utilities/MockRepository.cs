using ECommerce.OrderService.Application.Common;
using System.Linq.Expressions;

namespace ECommerce.OrderService.UnitTests.Utilities;

public class MockRepository<T> : Mock<IRepository<T>> where T : class
{
    private readonly List<T> _entities = new();

    public MockRepository()
    {
        Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _entities.AsReadOnly());

        Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) => _entities.Add(entity))
            .Returns(Task.CompletedTask);

        Setup(x => x.UpdateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Setup(x => x.DeleteAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) => _entities.Remove(entity))
            .Returns(Task.CompletedTask);
    }

    public void AddEntity(T entity)
    {
        _entities.Add(entity);
    }

    public void AddEntities(IEnumerable<T> entities)
    {
        _entities.AddRange(entities);
    }

    public void ClearEntities()
    {
        _entities.Clear();
    }

    public IReadOnlyList<T> GetEntities()
    {
        return _entities.AsReadOnly();
    }
}