using Marten;
using Wolverine.Http;
using static SimpleBettingExchange.Markets.MarketServices;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, string StartTime);
public record CreateMarketLineRequest(string Name);

public record MarketCreationResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

public record CreateMarketCommand(string Name, DateTimeOffset StartTime);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<(MarketCreationResponse, MarketCreated)> HandleAsync(CreateMarketCommand createMarket, 
        IPersistMarket persistMarketToDatabase)
    {
        var id = Guid.NewGuid();
        
        var created = await persistMarketToDatabase.Persist(id, id => Handle(id, createMarket)) as MarketCreated;
        
        return (
            new MarketCreationResponse(created.Id),
            created
        );
    }
}