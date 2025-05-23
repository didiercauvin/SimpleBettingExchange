using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SimpleBettingExchange.Markets;

public static class CloseMarketEndPoint
{
    public static IEndpointRouteBuilder UseCloseMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets/{marketId:Guid}/close", async (Guid marketId, IGrainFactory grainFactory) =>
        {
            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.CloseMarket(new CloseMarketCommand(marketId, DateTimeOffset.Now));
        });

        return endpoints;
    }
}

[GenerateSerializer]
public record CloseMarketCommand(Guid MarketId, DateTimeOffset Date);
