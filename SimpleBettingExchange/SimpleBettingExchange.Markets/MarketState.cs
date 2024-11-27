namespace SimpleBettingExchange.Markets;

public record MarketCreated(Guid Id, string Name, MarketLineState[] Lines, DateTimeOffset CreatedAt) : IEvent;

[GenerateSerializer]
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

[GenerateSerializer]
public class MarketLineState(Guid id, string name)
{
    [Id(0)] public Guid Id { get; } = id;
    [Id(1)] public string Name { get; } = name;
}
