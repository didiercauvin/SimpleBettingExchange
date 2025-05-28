using Marten;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, string StartTime);
public record CreateMarketLineRequest(string Name);

public record MarketCreationResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

public record CreateMarketCommand(string EventName, string Name, DateTimeOffset StartTime);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<(MarketCreationResponse, MarketCreated)> HandleAsync(CreateMarketCommand createMarket, 
        IPersistMarket persistMarketToDatabase)
    {
        var id = Guid.NewGuid();
        
        var created = await persistMarketToDatabase.Persist<MarketCreated>(id, id => Handle(id, createMarket));
        
        return (
            new MarketCreationResponse(created.Id),
            created
        );
    }

    private static MarketCreated Handle(Guid id, CreateMarketCommand createMarket)
    {
        var (eventName, name, lines) = createMarket;
        return new MarketCreated(id, eventName, name, createMarket.StartTime, DateTimeOffset.Now);
    }
}