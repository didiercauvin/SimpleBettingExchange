using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public record ChangeMarketNameRequest(string Name);

[GenerateSerializer]
public record ChangeMarketNameCommand(Guid Id, string Name);

public static class ChangeMarketNameEndPoint
{
    public static IEndpointRouteBuilder UseChangeMarketNameEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/markets/{marketId:Guid}", async (Guid marketId, ChangeMarketNameRequest request,IGrainFactory grainFactory) =>
        {
            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.ChangeName(new ChangeMarketNameCommand(marketId, request.Name));
        });

        return endpoints;
    }
}
