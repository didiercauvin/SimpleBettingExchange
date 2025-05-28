using SimpleBettingEchange.Markets.Contracts;

namespace SimpleBettingExchange.Markets;

public class PlaceBetService : IPlaceBetService
{
    private readonly IGrainFactory _grainFactory;

    public PlaceBetService(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }
    
    public async Task<BetPlacementResult> PlaceBackBet(Guid traderId, Guid marketId, Guid selectionId, decimal odds, decimal stake)
    {
        var grain = _grainFactory.GetGrain<MarketGrain>(marketId);
        
        var result = await grain.PlaceBackBet(traderId, selectionId, odds, stake);
        
        return new BetPlacementResult(result.IsAccepted, result.Reason);
    }

    public Task<BetPlacementResult> PlaceLayBet(Guid traderId, Guid marketId, Guid selectionId, decimal odds, decimal stake)
    {
        throw new NotImplementedException();
    }
}
