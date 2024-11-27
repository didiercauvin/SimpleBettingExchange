namespace SimpleBettingExchange.Markets;

public interface IMarketRepository
{
    Task Add(Market market, CancellationToken cancellationToken);
}

public class OrleansMarketRepository : IMarketRepository
{
    private readonly IGrainFactory _grainFactory;

    public OrleansMarketRepository(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task Add(Market market, CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IMarketGrain>(market.Id);
        await grain.Create(market.Id, market.Name, market.Lines.Select(l => new MarketLineState(l.Id, l.Name)).ToArray());
    }
}