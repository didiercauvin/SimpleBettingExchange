using Marten;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string EventName, string Name, DateTimeOffset StartTime);

public record MarketCreationResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

[GenerateSerializer]
public record CreateMarketCommand(string EventName, string Name, DateTimeOffset StartTime);

public static class CreateMarketEndPoint
{
    [WolverinePost("/api/markets")]
    public static async Task<(MarketCreationResponse, MarketCreated)> HandleAsync(CreateMarketCommand command, 
        IGrainFactory grains)
    {
        var id = Guid.NewGuid();    
        
        var grain = grains.GetGrain<IMarketGrain>(id);
        var created = await grain.Handle(command);
        
        return (
            new MarketCreationResponse(created.Id),
            created
        );
    }
}