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
            MarketNameChanged nameChanged => Handle(id, nameChanged),
            _ => throw new ArgumentException("Unknown type of event", nameof(@event)),
        };
    }

    public async Task<Market> GetMarketById(Guid id)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        
        var state = await grain.GetMarketState();
        return state.ToMarket();
    }

    private async Task Handle(Guid id, MarketCreated created)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateCreated(created.Id, created.Name, created.StartTime, created.CreatedAt));
    }

    private async Task Handle(Guid id, MarketNameChanged @event)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateNameChanged(@event.Id, @event.Name));
    }
}