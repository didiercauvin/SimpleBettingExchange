using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public static class SuspendMarketEndPoint
{
    public static IEndpointRouteBuilder UseSuspendMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets/{marketId:Guid}/suspend", async (Guid marketId, IGrainFactory grainFactory) =>
        {
            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.SuspendMarket(new SuspendMarketCommand(marketId, DateTimeOffset.Now));
        });

        return endpoints;
    }
}

[GenerateSerializer]
public record SuspendMarketCommand(Guid MarketId, DateTimeOffset Date);
