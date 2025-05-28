using SimpleBettingEchange.Core;

namespace SimpleBettingExchange.Markets;

[GenerateSerializer]
public record MarketCreated(Guid Id, string EventName, string Name, DateTimeOffset StartTime, DateTimeOffset CreatedAt) : IEvent;
[GenerateSerializer]
public record MarketNameChanged(Guid Id, string Name) : IEvent;
[GenerateSerializer]
public record MarketRunnersAdded(Guid MarketId, RunnerSnapshot[] Runners) : IEvent;
[GenerateSerializer]
public record RunnerSnapshot(Guid RunnerId, string Name, PriceSnapshot BackPrice, PriceSnapshot LayPrice);
[GenerateSerializer]
public record PriceSnapshot(decimal Price, decimal Size);

public interface IBackBetEvent : IEvent
{
    
}
[GenerateSerializer]
public record BackBetPlaced() : IBackBetEvent;
[GenerateSerializer]
public record BackBetRejected() : IBackBetEvent;

[GenerateSerializer]
public record MarketSuspended(Guid MarketId, DateTimeOffset Date) : IEvent;
[GenerateSerializer]
public record MarketResumed(Guid MarketId, DateTimeOffset Date) : IEvent;
[GenerateSerializer]
public record MarketClosed(Guid MarkerId, DateTimeOffset Date) : IEvent;

public enum MarketStatus { Created, Opened, Suspended, Closed }

[GenerateSerializer]
public class Market
{
    [Id(0)]
    public Guid Id { get; set; }
    
    [Id(1)]
    public string EventName { get; set; }
    
    [Id(2)]
    public string Name { get; set; }
    
    [Id(3)]
    public MarketStatus Status { get; set; }
    
    [Id(4)]
    public DateTimeOffset StartTime { get; set; }
    
    [Id(5)]
    public DateTimeOffset? EndTime { get; set; }

    [Id(6)]
    public Runner[] Lines { get; set; } = [];

    public static Market None => new();

    public static Market When(Market state, IEvent @event)
    {
        return @event switch
        {
            MarketCreated created => state.Apply(created),
            MarketNameChanged nameChanged => state.Apply(nameChanged),
            MarketRunnersAdded runnersAdded => state.Apply(runnersAdded),
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
        EventName = created.EventName;
        Name = created.Name;
        Status = MarketStatus.Created;
        StartTime = created.StartTime;

        return this;
    }

    private Market Apply(MarketNameChanged nameChanged)
    {
        Name = nameChanged.Name;
        
        return this;
    }

    private Market Apply(MarketRunnersAdded runnersAdded)
    {
        var currentRunners = Lines.ToList();
        
        currentRunners.AddRange(
            runnersAdded.Runners.Select(r => 
                new Runner(
                    r.RunnerId, 
                    r.Name, 
                    [new Price(r.BackPrice.Price, r.BackPrice.Size)], 
                    [new Price(r.LayPrice.Price, r.LayPrice.Size)]
                )
            )
        );
        
        Lines = currentRunners.ToArray();

        return this;
    }
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

[GenerateSerializer]
public class Runner(Guid id, string name, Price[] backPrices, Price[] layPrices)
{
    [Id(0)]
    public Guid Id { get; } = id;
    [Id(1)]
    public string Name { get; } = name;

    [Id(2)]
    public Price[] BackPrices { get;  } = backPrices;
    [Id(3)]
    public Price[] LayPrices { get; set; } = layPrices;
}

[GenerateSerializer]
public record Price(decimal PriceValue, decimal Size);