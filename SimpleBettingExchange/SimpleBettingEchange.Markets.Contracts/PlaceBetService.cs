namespace SimpleBettingEchange.Markets.Contracts;

public interface IPlaceBetService
{
    Task<BetPlacementResult> PlaceBackBet(Guid traderId, Guid marketId, Guid selectionId, decimal odds, decimal stake);
    Task<BetPlacementResult> PlaceLayBet(Guid traderId, Guid marketId, Guid selectionId, decimal odds, decimal stake);
}

public record BetPlacementResult(bool IsAccepted, string? Reason = null);