namespace SimpleBettingExchange.Markets;

public record MarketCreated(Guid Id, string Name, DateTimeOffset StartTime, DateTimeOffset CreatedAt) : IEvent;
public record MarketNameChanged(Guid Id, string Name) : IEvent;
public record MarketRunnersAdded(Guid MarketId, Runner[] Runners) : IEvent;

public enum MarketStatus { Created, Opened, Suspended, Closed }

[GenerateSerializer]
public class MarketState
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MarketStatus Status { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public Runner[] Lines { get; set; } = [];

    public void When(IEvent @event)
    {
        switch (@event)
        {
            case MarketCreated created: Apply(created); break;
            case MarketNameChanged nameChanged: Apply(nameChanged); break;
            case MarketRunnersAdded runnersAdded: Apply(runnersAdded); break;
        }
    }

    public void Apply(MarketCreated created)
    {
        Id = created.Id;
        Name = created.Name;
        Status = MarketStatus.Created;
        StartTime = created.StartTime;
    }

    public void Apply(MarketNameChanged nameChanged)
    {
        Name = nameChanged.Name;
    }

    public void Apply(MarketRunnersAdded runnersAdded)
    {
        var currentRunners = Lines.ToList();
        currentRunners.AddRange(runnersAdded.Runners.Select(r => new Runner(r.Id, r.Name, r.BackPrices, r.LayPrices)));
        Lines = currentRunners.ToArray();
    }
}

[GenerateSerializer]
public class Runner(Guid id, string name, Price[] backPrices, Price[] layPrices)
{
    [Id(0)] public Guid Id { get; } = id;
    [Id(1)] public string Name { get; } = name;

    [Id(2)] public Price[] BackPrices { get;  } = backPrices;
    [Id(3)] public Price[] LayPrices { get; set; } = layPrices;
}

[GenerateSerializer]
public record Price(decimal PriceValue, decimal Size);