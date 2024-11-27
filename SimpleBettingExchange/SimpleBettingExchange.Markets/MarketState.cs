namespace SimpleBettingExchange.Markets;

public record MarketCreated(Guid Id, string Name, MarketLineState[] Lines, DateTimeOffset CreatedAt) : IEvent;
public record MarketNameChanged(Guid Id, string Name) : IEvent;

[GenerateSerializer]
public class MarketState
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MarketStatus Status { get; set; }
    public MarketLineState[] Lines { get; set; }

    public void When(IEvent @event)
    {
        switch (@event)
        {
            case MarketCreated created: Apply(created); break;
            case MarketNameChanged nameChanged: Apply(nameChanged); break;
        }
    }

    public void Apply(MarketCreated created)
    {
        Id = created.Id;
        Name = created.Name;
        Status = MarketStatus.Created;
        Lines = created.Lines.ToArray();
    }

    public void Apply(MarketNameChanged nameChanged)
    {
        Name = nameChanged.Name;
    }
}

[GenerateSerializer]
public class MarketLineState(Guid id, string name)
{
    [Id(0)] public Guid Id { get; } = id;
    [Id(1)] public string Name { get; } = name;
}
