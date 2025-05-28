using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record ChangeMarketNameRequest(Guid Id, string Name);

public delegate Task<Market> GetMarketById(Guid id);

public static class ChangeMarketNameEndPoint
{
    [WolverinePut("/api/markets/{marketId:Guid}"), EmptyResponse]
    public static async Task<MarketNameChanged> ChangeMarketName(Guid marketId, ChangeMarketNameCommand command, 
        IGrainFactory grains)
    {
        var grain = grains.GetGrain<IMarketGrain>(marketId);
        var created = await grain.Handle(command);
        
        return created;
    }
}
