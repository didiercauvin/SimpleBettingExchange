using Marten;
using Wolverine.Http;
using static SimpleBettingExchange.Markets.MarketServices;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, string StartTime);
public record CreateMarketLineRequest(string Name);

public record MarketCreationResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

public record CreateMarketCommand(string Name, DateTimeOffset StartTime);

public delegate Task PeristsMarketToDatabase(Guid id, IEvent created);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<(MarketCreationResponse, MarketCreated)> HandleAsync(CreateMarketCommand createMarket, 
        PeristsMarketToDatabase persistMarketToDatabase)
    {
        var created = Handle(createMarket);
        
        await persistMarketToDatabase(created.Id, created);
        
        return (
            new MarketCreationResponse(created.Id),
            created
        );
    }
}