namespace SimpleBettingExchange.Markets;

public abstract class Entity
{
    private List<object> _events = new();

    public IEnumerable<object> GetChanges() => _events.AsEnumerable();
    
    public void ClearChanges() => _events.Clear();
    
    protected abstract void When(object @event);
    
    protected void Apply(object @event)
    {
        When(@event);
        _events.Add(@event);
    }
}