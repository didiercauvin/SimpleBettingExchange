using Wolverine.Http;

namespace SimpleBettingExchange.Markets.SubmittingBackBet;

public record SubmitBackBetCommand(Guid TraderId, decimal Odds, decimal Stake);

public static class SubmitBackEndpoint
{
    [WolverinePost("/api/market/{marketId: Guid}/back/{selectionId:Guid}")]
    public static async Task<IBackBetEvent> SubmitBack(Guid marketId, Guid selectionId, 
        SubmitBackBetCommand command,
        IPersistMarket persistMarket)
    {
        return await persistMarket.GetAndUpdate<IBackBetEvent>(
            marketId, 
            market => Handle(market, selectionId, command));
    }

    private static IBackBetEvent Handle(Market market, Guid selectionId,
        SubmitBackBetCommand command)
    {
        return new BackBetPlaced();
    }
}