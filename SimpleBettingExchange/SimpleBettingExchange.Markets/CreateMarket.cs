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
        endpoints.MapPost("/api/markets", async (CreateMarketRequest body, IMarketRepository repository) =>
        {
            var marketId = Guid.NewGuid();

            await MarketServices.Handle(new CreateMarket(marketId, body.Name, body.Lines.Select(l => new CreateMarketLine(Guid.NewGuid(), l.Name)).ToArray()), repository, CancellationToken.None);

            return Created($"/api/markets/{marketId}", marketId);
        });
        
        return endpoints;
    }
}


public record CreateMarket(Guid Id, string Name, CreateMarketLine[] Lines);

public class CreateMarketLine
{
    public CreateMarketLine(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
}
