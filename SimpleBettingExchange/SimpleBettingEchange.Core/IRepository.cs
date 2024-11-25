namespace SimpleBettingExchange.Markets;

public interface IRepository<in T> where T : Entity
{
    Task Add(Guid id, T entity, CancellationToken cancellationToken);
}