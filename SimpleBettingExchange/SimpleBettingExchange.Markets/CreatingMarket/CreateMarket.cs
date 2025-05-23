using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, string StartTime);
public record CreateMarketLineRequest(string Name);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<MarketCreated> HandleAsync(CreateMarketCommand createMarket, IDocumentSession session, IGrainFactory grains)
    {
        var (id, name, lines) = createMarket;

        var created = new MarketCreated(id, name, createMarket.StartTime, DateTimeOffset.Now);

        var grain = grains.GetGrain<IMarketGrain>(id);
        await grain.Handle(created);

        session.Events.StartStream<MarketState>(id, created);
        await session.SaveChangesAsync();

        return created;
    }

    public static IEndpointRouteBuilder UseCreateMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets", async (CreateMarketRequest body, IGrainFactory grainFactory) =>
        {
            var marketId = Guid.NewGuid();

            var marketGrain = grainFactory.GetGrain<IMarketGrain>(marketId);

            await marketGrain.CreateMarket(new CreateMarketCommand(marketId, body.Name, DateTimeOffset.Parse(body.StartTime)));

            return Created($"/api/markets/{marketId}", marketId);
        });
        
        return endpoints;
    }
}

[GenerateSerializer]
public record CreateMarketCommand(Guid Id, string Name, DateTimeOffset StartTime);

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
