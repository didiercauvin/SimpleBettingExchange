using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    public static MarketCreated Handle(Guid id, CreateMarketCommand createMarket)
    {
        var (name, lines) = createMarket;
        return new MarketCreated(id, name, createMarket.StartTime, DateTimeOffset.Now);
    }

    public static MarketNameChanged Handle(Market market, ChangeMarketNameCommand changeMarketName)
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

    public static MarketSuspended Handle(SuspendMarketCommand command)
    {
        return new MarketSuspended(command.MarketId, command.Date);
    }

    public static MarketResumed Handle(ResumeMarketCommand command)
    {
        return new MarketResumed(command.MarketId, command.Date);
    }

    public static MarketClosed Handle(CloseMarketCommand command)
    {
        return new MarketClosed(command.MarketId, command.Date);
    }
}