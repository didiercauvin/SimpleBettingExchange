using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, CreateMarketLineRequest[] Lines);
public record CreateMarketLineRequest(string Name);

public static class MarketEndPoints
{
    public static IEndpointRouteBuilder UseCreateMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets", async (CreateMarketRequest body, IGrainFactory grainFactory) =>
        {
            var marketId = Guid.NewGuid();
            var grain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await grain.Create(new CreateMarketCommand(body.Name, body.Lines.Select(l => new CreateMarketLine(Guid.NewGuid(), l.Name)).ToArray()));

            return Created($"/api/markets/{marketId}", marketId);
        });
        
        return endpoints;
    }
}

[GenerateSerializer]
public record CreateMarketCommand(string Name, CreateMarketLine[] Lines);

[GenerateSerializer]
public class CreateMarketLine
{
    public CreateMarketLine(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string Name { get; set; }
}
