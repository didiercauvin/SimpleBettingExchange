using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    public static MarketCreated Handle(CreateMarketCommand createMarket)
    {
        var (id, name, lines) = createMarket;
        return new MarketCreated(id, name, createMarket.StartTime, DateTimeOffset.Now);
    }

    public static MarketNameChanged Handle(ChangeMarketNameCommand changeMarketName)
    {
        return new MarketNameChanged(changeMarketName.Id, changeMarketName.Name);
    }

    public static MarketRunnersAdded Handle(AddRunnersCommand command)
    {
        return new MarketRunnersAdded(
            command.MarketId,
            command.Runners.Select(r => new Runner(
                r.Id,
                r.Name,
                [new Price(r.BackPrice.Price, r.BackPrice.Size)],
                [new Price(r.LayPrice.Price, r.LayPrice.Size)])).ToArray());
    }
}