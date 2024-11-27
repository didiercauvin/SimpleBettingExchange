namespace SimpleBettingExchange.Markets;

public record MarketCreated(Guid Id, string Name, IEnumerable<MarketLineState> Lines, DateTimeOffset CreatedAt) : IEvent;

public enum MarketStatus { Created, Opened, Suspended, Closed }

public class MarketState
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MarketStatus State { get; set; }
    public MarketLineState[] Lines { get; set; }

    public void When(IEvent @event)
    {
        switch (@event)
        {
            case MarketCreated created: Apply(created); break;
        }
    }

    public void Apply(MarketCreated created)
    {
        Id = created.Id;
        Name = created.Name;
        State = MarketStatus.Created;
        Lines = created.Lines.ToArray();
    }
}

public class MarketLineState(Guid id, string name)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
}
