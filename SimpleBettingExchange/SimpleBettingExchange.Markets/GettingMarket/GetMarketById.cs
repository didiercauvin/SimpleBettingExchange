using Marten;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets.GettingMarket;

public record GetMarketByIdResponse(Guid Id, string EventName, string Name, DateTimeOffset StartTime, GetMarketRunnerResponse[] Runners);
public record GetMarketRunnerResponse(Guid Id, string Name, GetMarketRunnerPriceResponse[] BackPrices, GetMarketRunnerPriceResponse[] LayPrices);
public record GetMarketRunnerPriceResponse(decimal Price, decimal Size); 

public class GetAllMarkets
{
    [WolverineGet("/api/markets/{id}")]
    public static async Task<GetMarketByIdResponse> Get(Guid id, IGrainFactory grains)
    {
        var marketGrain = grains.GetGrain<IMarketGrain>(id);
        var state = await marketGrain.GetMarketState();

        return new GetMarketByIdResponse(state.Id, state.EventName, state.Name, state.StartTime, 
            state.Lines.Select(l => new GetMarketRunnerResponse(
                l.Id, 
                l.Name, 
                l.BackPrices.Select(p => new GetMarketRunnerPriceResponse(p.PriceValue, p.Size)).ToArray(), 
                l.LayPrices.Select(p => new GetMarketRunnerPriceResponse(p.PriceValue, p.Size)).ToArray())
            ).ToArray()
        );
    }
}