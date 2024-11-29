using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SimpleBettingExchange.Markets;

public static class ResumeMarketEndPoint
{
    public static IEndpointRouteBuilder UseResumeMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets/{marketId:Guid}/resume", async (Guid marketId, IGrainFactory grainFactory) =>
        {
            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.ResumeMarket(new ResumeMarketCommand(marketId, DateTimeOffset.Now));
        });

        return endpoints;
    }
}

[GenerateSerializer]
public record ResumeMarketCommand(Guid MarketId, DateTimeOffset Date);
