using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;
using static SimpleBettingExchange.Markets.MarketServices;

namespace SimpleBettingExchange.Markets;

public record ChangeMarketNameRequest(string Name);
public record ChangeMarketNameResponse(Guid Id) : CreationResponse("/api/markets/" + Id);

public record ChangeMarketNameCommand(Guid Id, string Name);

public delegate Task<Market> GetMarketById(Guid id);

public static class ChangeMarketNameEndPoint
{
    [WolverinePut("/api/markets/{marketId:Guid}"), EmptyResponse]
    public static async Task<MarketNameChanged> ChangeMarketName(Guid marketId, ChangeMarketNameCommand changeMarketName,
        IPersistMarket peristsMarketToDatabase)
    {
        var @event = await peristsMarketToDatabase.GetAndUpdate<MarketNameChanged>(
            marketId, 
            market => Handle(market, changeMarketName)
        );
        
        return @event;
    }
}
