using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, CreateMarketLineRequest[] Lines);
public record CreateMarketLineRequest(string Name);

public static class MarketEndPoints
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

public static class MarketServices
{
    public static async Task Handle(CreateMarket createMarket, IMarketRepository repository, CancellationToken ct)
    {
        var market = Market.Create(createMarket.Id, createMarket.Name, createMarket.Lines.Select(l => new MarketLine(Guid.NewGuid(), l.Name)));

        await repository.Add(market, ct);
    }
}

public enum MarketStatus { Created, Opened, Suspended, Closed }

public class Market
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MarketStatus Status { get; set; }
    public MarketLine[] Lines { get; set; }

    private Market(Guid id, string name, IEnumerable<MarketLine> lines)
    {
        Id = id;
        Name = name;
        Status = MarketStatus.Created;
        Lines = lines.ToArray();
    }

    public static Market Create(Guid id, string name, IEnumerable<MarketLine> lines)
        => new Market(id, name, lines);
}

public class MarketLine
{
    public MarketLine(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
}
