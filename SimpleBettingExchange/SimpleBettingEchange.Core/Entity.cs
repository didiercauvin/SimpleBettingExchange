namespace SimpleBettingExchange.Markets;

public abstract class Entity
{
    private List<object> _events = new();

    public IEnumerable<object> GetChanges() => _events.AsEnumerable();
    
    public void ClearChanges() => _events.Clear();

    public abstract void When(IEvent @event);

    protected void Apply(IEvent @event)
    {
        When(@event);
        _events.Add(@event);
    }
}