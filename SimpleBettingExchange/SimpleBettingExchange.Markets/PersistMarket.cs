namespace SimpleBettingExchange.Markets;

public class PersistMarket
{
    private readonly IGrainFactory _grains;

    public PersistMarket(IGrainFactory grains)
    {
        _grains = grains;
    }
    
    public Task Persist(Guid id, IEvent @event)
    {
        return @event switch
        {
            MarketCreated created => Handle(id, created),
            _ => throw new ArgumentException("Unknown type of event", nameof(@event)),
        };
    }

    private async Task Handle(Guid id, MarketCreated created)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateCreated(created.Id, created.Name, created.StartTime, created.CreatedAt));
    }
}