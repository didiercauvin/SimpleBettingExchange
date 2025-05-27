namespace SimpleBettingExchange.Markets;

[GenerateSerializer]
public record MarketStateCreated(Guid Id, string Name, DateTimeOffset StartTime, DateTimeOffset CreatedAt);

public record MarketCreated(Guid Id, string Name, DateTimeOffset StartTime, DateTimeOffset CreatedAt) : IEvent;
public record MarketNameChanged(Guid Id, string Name) : IEvent;
public record MarketRunnersAdded(Guid MarketId, Runner[] Runners) : IEvent;
public record MarketSuspended(Guid MarketId, DateTimeOffset Date) : IEvent;
public record MarketResumed(Guid MarketId, DateTimeOffset Date) : IEvent;
public record MarketClosed(Guid MarkerId, DateTimeOffset Date) : IEvent;

public enum MarketStatus { Created, Opened, Suspended, Closed }

public class Market
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public MarketStatus Status { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public Runner[] Lines { get; set; } = [];

    public static Market None => new();

    public static Market When(Market state, IEvent @event)
    {
        return @event switch
        {
            MarketCreated created => state.Apply(created),
            _ => throw new ArgumentException("Unknown type of event", nameof(@event))
            // case MarketNameChanged nameChanged: Apply(nameChanged); break;
            // case MarketRunnersAdded runnersAdded: Apply(runnersAdded); break;
            // case MarketSuspended suspended: Apply(suspended); break;
            // case MarketResumed resumed: Apply(resumed); break;
            // case MarketClosed closed: Apply(closed); break;
        };
    }

    private Market Apply(MarketCreated created)
    {
        Id = created.Id;
        Name = created.Name;
        Status = MarketStatus.Created;
        StartTime = created.StartTime;

        return this;
    }

    // private static Market Apply(MarketCreated created)
    // {
    //     return new Market()
    //     {
    //         Id = created.Id,
    //         Name = created.Name,
    //         Status = MarketStatus.Created,
    //         StartTime = created.StartTime,
    //     };
    // }

    // public void Apply(MarketNameChanged nameChanged)
    // {
    //     Name = nameChanged.Name;
    // }
    //
    // public void Apply(MarketRunnersAdded runnersAdded)
    // {
    //     // var currentRunners = Lines.ToList();
    //     // currentRunners.AddRange(runnersAdded.Runners.Select(r => new RunnerState(r.Id, r.Name, r.BackPrices, r.LayPrices)));
    //     // Lines = currentRunners.ToArray();
    // }
    //
    // public void Apply(MarketSuspended suspended)
    // {
    //     Status = MarketStatus.Suspended;
    // }
    //
    // public void Apply(MarketResumed resumed)
    // {
    //     Status = MarketStatus.Opened;
    // }
    //
    // public void Apply(MarketClosed closed)
    // {
    //     Status = MarketStatus.Closed;
    //     EndTime = closed.Date;
    // }
}

public class Runner(Guid id, string name, Price[] backPrices, Price[] layPrices)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;

    public Price[] BackPrices { get;  } = backPrices;
    public Price[] LayPrices { get; set; } = layPrices;
}

public record Price(decimal PriceValue, decimal Size);