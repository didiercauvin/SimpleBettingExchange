using SimpleBettingEchange.Core;

namespace SimpleBettingExchange.Markets;

public interface IPersistMarket
{
    Task<TEvent> Persist<TEvent>(Guid id, Func<Guid, TEvent> handler) where TEvent : IEvent;
    Task<Market> GetMarketById(Guid id);
    Task<TEvent> GetAndUpdate<TEvent>(Guid id, Func<Market, TEvent> handler) where TEvent : IEvent;
}

public class PersistMarket : IPersistMarket
{
    private readonly IGrainFactory _grains;

    public PersistMarket(IGrainFactory grains)
    {
        _grains = grains;
    }
    
    public Task<TEvent> Persist<TEvent>(Guid id, Func<Guid, TEvent> handler) where TEvent : IEvent
    {
        var @event = handler(id);
        
        return Persist(id, @event);
    }

    public async Task<TEvent> GetAndUpdate<TEvent>(Guid id, Func<Market, TEvent> handler) where TEvent : IEvent
    {
        var market = await GetMarketById(id);
        var @event = handler(market);
        
        await Persist(id, @event);
        
        return @event;
    }

    public async Task<Market> GetMarketById(Guid id)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        
        var state = await grain.GetMarketState();
        return state.ToMarket();
    }
    
    private async Task<TEvent> Persist<TEvent>(Guid id, TEvent @event) where TEvent : IEvent
    {
        return @event switch
        {
            MarketCreated created => (TEvent)(IEvent)(await Handle(id, created)),
            MarketNameChanged nameChanged => (TEvent)(IEvent)(await Handle(id, nameChanged)),
            MarketRunnersAdded runnersAdded => (TEvent)(IEvent)(await Handle(id, runnersAdded)),
            _ => throw new ArgumentException("Unknown type of event", nameof(@event)),
        };
    }

    private async Task<MarketCreated> Handle(Guid id, MarketCreated created)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateCreated(created.Id, created.EventName, created.Name, created.StartTime, created.CreatedAt));

        return created;
    }

    private async Task<IEvent> Handle(Guid id, MarketNameChanged @event)
    {
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(new MarketStateNameChanged(@event.Id, @event.Name));

        return @event;
    }

    private async Task<IEvent> Handle(Guid id, MarketRunnersAdded runnersAdded)
    {
        var @event = new MarketStateRunnersAdded(
            runnersAdded.MarketId, 
            runnersAdded.Runners.Select(r => new RunnerStateSnapshot(
                r.RunnerId,
                r.Name,
                new PriceStateSnapshot(r.BackPrice.Price, r.BackPrice.Size),
                new PriceStateSnapshot(r.LayPrice.Price, r.LayPrice.Size))).ToArray());
        
        var grain = _grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(@event);

        return runnersAdded;
    }
}