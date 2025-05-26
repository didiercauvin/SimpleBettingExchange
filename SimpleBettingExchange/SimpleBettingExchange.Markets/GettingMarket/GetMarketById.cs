using Marten;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets.GettingMarket;

public record GetMarketByIdResponse(Guid Id, string Name, DateTimeOffset StartTime);

public class GetAllMarkets
{
    [WolverineGet("/api/markets/{id}")]
    public static async Task<GetMarketByIdResponse> Get(Guid id, IGrainFactory grains)
    {
        var marketGrain = grains.GetGrain<IMarketGrain>(id);
        var state = await marketGrain.GetMarketState();

        return new GetMarketByIdResponse(state.Id, state.Name, state.StartTime);
    }
}