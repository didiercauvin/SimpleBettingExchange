using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;
using static SimpleBettingExchange.Markets.MarketServices;

namespace SimpleBettingExchange.Markets;

public record AddRunnersRequest(IEnumerable<AddRunnerItemRequest> Runners);
public record AddRunnerItemRequest(string Name, AddRunnerPriceRequest BackPrice, AddRunnerPriceRequest LayPrice);

public record AddRunnerPriceRequest(decimal Price, decimal Size);

public record AddRunnersCommand(Guid MarketId, AddRunnerItemCommand[] Runners);
public record AddRunnerItemCommand(string Name, AddRunnerPriceCommand BackPrice, AddRunnerPriceCommand LayPrice);
public record AddRunnerPriceCommand(decimal Price, decimal Size);

public static class AddRunnersEndPoint
{
    [WolverinePost("/api/markets/{marketId:Guid}/runners")]
    public static Task<MarketRunnersAdded> AddRunners(Guid marketId, AddRunnersCommand command,
        IPersistMarket persistMarket)
    {
        var runnerId = Guid.NewGuid();
        var @event = persistMarket.GetAndUpdate(
            marketId,
            market => Handle(market, runnerId, command)
        );

        return @event;
    }

    private static MarketRunnersAdded Handle(Market market, Guid runnerId, AddRunnersCommand command)
    {
        return new MarketRunnersAdded(
            command.MarketId,
            command.Runners.Select(r => new RunnerSnapshot(
                runnerId,
                r.Name,
                new PriceSnapshot(r.BackPrice.Price, r.BackPrice.Size),
                new PriceSnapshot(r.LayPrice.Price, r.LayPrice.Size))
            ).ToArray()
        );
    }
}