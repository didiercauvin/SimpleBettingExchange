using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public record AddRunnersRequest(IEnumerable<AddRunnerItemRequest> Runners);
public record AddRunnerItemRequest(string Name, AddRunnerPriceRequest BackPrice, AddRunnerPriceRequest LayPrice);

public record AddRunnerPriceRequest(decimal Price, decimal Size);

public static class AddRunnersEndPoint
{
    public static IEndpointRouteBuilder UseAddRunnersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets/{marketId:Guid}/runners", async (Guid marketId, AddRunnersRequest body, IGrainFactory grainFactory) =>
        {
            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.AddRunners(
                new AddRunnersCommand(
                    marketId, 
                    body.Runners.Select(r => new AddRunnerItemCommand(
                        Guid.NewGuid(), 
                        r.Name, 
                        new AddRunnerPriceCommand(r.BackPrice.Price, r.BackPrice.Size),
                        new AddRunnerPriceCommand(r.LayPrice.Price, r.LayPrice.Size))
                    ).ToArray()
                )
            );
        });

        return endpoints;
    }
}

[GenerateSerializer]
public record AddRunnersCommand(Guid MarketId, AddRunnerItemCommand[] Runners);
[GenerateSerializer]
public record AddRunnerItemCommand(Guid Id, string Name, AddRunnerPriceCommand BackPrice, AddRunnerPriceCommand LayPrice);
[GenerateSerializer]
public record AddRunnerPriceCommand(decimal Price, decimal Size);
