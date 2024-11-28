using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, CreateMarketLineRequest[] Lines);
public record CreateMarketLineRequest(string Name);

public static class CreateMarketEndPoint
{
    public static IEndpointRouteBuilder UseCreateMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets", async (CreateMarketRequest body, IGrainFactory grainFactory) =>
        {
            var marketId = Guid.NewGuid();

            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.CreateMarket(new CreateMarketCommand(marketId, body.Name, body.Lines.Select(l => new CreateMarketLineCommand(Guid.NewGuid(), l.Name)).ToArray()));

            return Created($"/api/markets/{marketId}", marketId);
        });
        
        return endpoints;
    }
}

[GenerateSerializer]
public record CreateMarketCommand(Guid Id, string Name, CreateMarketLineCommand[] Lines);

[GenerateSerializer]
public class CreateMarketLineCommand
{
    public CreateMarketLineCommand(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string Name { get; set; }
}
