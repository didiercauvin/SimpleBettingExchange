using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, CreateMarketLineRequest[] Lines);
public record CreateMarketLineRequest(string Name);

public static class MarketEndPoints
{
    public static IEndpointRouteBuilder UseCreateMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets", async (IGrainFactory grainFactory, CreateMarketRequest body) =>
        {
            var marketId = Guid.NewGuid();
            var grain = grainFactory.GetGrain<ICreateMarketGrain>(marketId);
            
            await grain.Create(marketId, body.Name, body.Lines.Select(l => new MarketLine(Guid.NewGuid(), l.Name)).ToArray());
        });
        
        return endpoints;
    }
}

public record CreateMarket(Guid Id, string Name, MarketLines Lines);

public interface ICreateMarketGrain : IGrainWithGuidKey
{
    Task Create(Guid id, string name, MarketLine[] lines);
}

public class CreateMarketGrain : Grain, ICreateMarketGrain
{
    private readonly IRepository<Market> _repository;
    public Market Market { get; set; }

    public CreateMarketGrain(IRepository<Market> repository)
    {
        _repository = repository;
    }
    
    public async Task Create(Guid id, string name, MarketLine[] lines)
    {
        await _repository.Add(id, Market.Create(id, name, lines), CancellationToken.None);
    }
}