using SimpleBettingEchange.Core;

namespace SimpleBettingExchange.Markets;

public record MarketCreated(Guid Id, string EventName, string Name, DateTimeOffset StartTime, DateTimeOffset CreatedAt) : IEvent;
public record MarketNameChanged(Guid Id, string Name) : IEvent;
public record MarketRunnersAdded(Guid MarketId, RunnerSnapshot[] Runners) : IEvent;
public record RunnerSnapshot(Guid RunnerId, string Name, PriceSnapshot BackPrice, PriceSnapshot LayPrice);
public record PriceSnapshot(decimal Price, decimal Size);

public interface IBackBetEvent : IEvent
{
    
}
public record BackBetPlaced() : IBackBetEvent;
public record BackBetRejected() : IBackBetEvent;


public record MarketSuspended(Guid MarketId, DateTimeOffset Date) : IEvent;
public record MarketResumed(Guid MarketId, DateTimeOffset Date) : IEvent;
public record MarketClosed(Guid MarkerId, DateTimeOffset Date) : IEvent;

public enum MarketStatus { Created, Opened, Suspended, Closed }

public class Market
{
    public Guid Id { get; set; }
    public string EventName { get; set; }

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

public class Runner(Guid id, string name, Price[] backPrices, Price[] layPrices)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;

    public Price[] BackPrices { get;  } = backPrices;
    public Price[] LayPrices { get; set; } = layPrices;
}

public record Price(decimal PriceValue, decimal Size);