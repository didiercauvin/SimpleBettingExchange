using Marten;
using Wolverine.Http;
using static SimpleBettingExchange.Markets.MarketServices;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, string StartTime);
public record CreateMarketLineRequest(string Name);

public record MarketCreationResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

[GenerateSerializer]
public record CreateMarketCommand(string Name, DateTimeOffset StartTime);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<(MarketCreationResponse, MarketCreated)> HandleAsync(CreateMarketCommand createMarket, 
        IDocumentSession session,
        IGrainFactory grains)
    {
        var id = Guid.NewGuid();
        var grain = grains.GetGrain<IMarketGrain>(id);
        var created = await grain.CreateMarket(createMarket.Name, createMarket.StartTime);
        
        return (
            new MarketCreationResponse(created.Id),
            created
        );
    }
}