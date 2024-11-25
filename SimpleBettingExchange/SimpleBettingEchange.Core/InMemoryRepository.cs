namespace SimpleBettingExchange.Markets;

public class InMemoryRepository<T> : IRepository<T> where T : Entity
{
    private Dictionary<Guid, List<object>> _entities = new();
    
    public Task Add(Guid id, T entity, CancellationToken cancellationToken)
    {
        if (!_entities.ContainsKey(id))
        {
            _entities.Add(id, entity.GetChanges().ToList());
        }
        else
        {
            _entities[id].Add(entity.GetChanges());
        }
        
        entity.ClearChanges();
        
        return Task.CompletedTask;
    }
}