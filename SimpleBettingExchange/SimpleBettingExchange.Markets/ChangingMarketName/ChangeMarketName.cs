using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;

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

    private static MarketNameChanged Handle(Market market, ChangeMarketNameCommand changeMarketName)
    {
        return new MarketNameChanged(changeMarketName.Id, changeMarketName.Name);
    }
}
