namespace SimpleBettingExchange.Markets;

public interface IMarketRepository
{
    Task Add(Market market, CancellationToken cancellationToken);
    Task ChangeName(Guid id, string name);
}

//public class OrleansMarketRepository : IMarketRepository
//{
//    private readonly IGrainFactory _grainFactory;

//    public OrleansMarketRepository(IGrainFactory grainFactory)
//    {
//        _grainFactory = grainFactory;
//    }

//    public async Task Add(Market market, CancellationToken cancellationToken)
//    {
//        var grain = _grainFactory.GetGrain<IMarketGrain>(market.Id);
//        await grain.CreateMarket(market.Id, market.Name, market.Lines.Select(l => new MarketLineState(l.Id, l.Name)).ToArray());
//    }

//    public async Task ChangeName(Guid id, string name)
//    {
//        var grain = _grainFactory.GetGrain<IMarketGrain>(id);

//        await grain.ChangeName(name);
//    }
//}

//public static class MarketStateExtensions
//{
//    public static Market ToMarket(this MarketState market)
//    {
//        return Market.From(market.Id, market.Name, market.Status, market.Lines.Select(l => new MarketLine(l.Id, l.Name)));
//    }
//}