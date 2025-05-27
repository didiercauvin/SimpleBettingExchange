namespace SimpleBettingExchange.Markets;

public interface IPersistMarket
{
    Task<IEvent> Persist(Guid id, Func<Guid, IEvent> handler);
    Task<Market> GetMarketById(Guid id);
    Task<IEvent> GetAndUpdate(Guid id, Func<Market, IEvent> handler);
}

public class PersistMarket : IPersistMarket
{
    private readonly IGrainFactory _grains;

    public PersistMarket(IGrainFactory grains)
    {
        _grains = grains;
    }
    
    public Task<IEvent> Persist(Guid id, Func<Guid, IEvent> handler)
    {
        var @event = handler(id);
        
        return Persist(id, @event);
    }
    
    private Task<IEvent> Persist(Guid id, IEvent @event)
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

    public async Task<IEvent> GetAndUpdate(Guid id, Func<Market, IEvent> handler)
    {
        var market = await GetMarketById(id);
        var @event = handler(market);
        
        await Persist(id, @event);
        
        return @event;
    }

    private async Task<IEvent> Handle(Guid id, MarketCreated created)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateCreated(created.Id, created.Name, created.StartTime, created.CreatedAt));

        return created;
    }

    private async Task<IEvent> Handle(Guid id, MarketNameChanged @event)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateNameChanged(@event.Id, @event.Name));

        return @event;
    }
}