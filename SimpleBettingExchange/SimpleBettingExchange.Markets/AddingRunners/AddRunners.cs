using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record AddRunnersRequest(IEnumerable<AddRunnerItemRequest> Runners);
public record AddRunnerItemRequest(string Name, AddRunnerPriceRequest BackPrice, AddRunnerPriceRequest LayPrice);
public record AddRunnerPriceRequest(decimal Price, decimal Size);

public static class AddRunnersEndPoint
{
    [WolverinePost("/api/markets/{marketId:Guid}/runners")]
    public static Task<MarketRunnersAdded> AddRunners(Guid marketId, AddRunnersCommand command,
        IGrainFactory grainFactory)
    {
        var grain = grainFactory.GetGrain<IMarketGrain>(marketId);
        var @event = grain.Handle(command);

        return @event;
    }

    
}