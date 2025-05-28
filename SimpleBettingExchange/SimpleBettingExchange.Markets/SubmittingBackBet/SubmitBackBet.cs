using Wolverine.Http;

namespace SimpleBettingExchange.Markets.SubmittingBackBet;

public record SubmitBackBetRequest(Guid TraderId, decimal Odds, decimal Stake);

public static class SubmitBackEndpoint
{
    [WolverinePost("/api/market/{marketId:Guid}/back/{selectionId:Guid}")]
    public static Task<IBackBetEvent> SubmitBack(Guid marketId, Guid selectionId, 
        SubmitBackBetCommand command,
        IGrainFactory grainFactory)
    {
        var grain = grainFactory.GetGrain<IMarketGrain>(marketId);
        var @event = grain.PlaceBackBet(command);
        return @event;
    }
}