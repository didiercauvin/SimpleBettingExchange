using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    public static MarketCreated Handle(CreateMarketCommand createMarket)
    {
        var (id, name, lines) = createMarket;
        return new MarketCreated(id, name, lines.Select(l => new MarketLineState(l.Id, l.Name)).ToArray(), DateTimeOffset.Now);
    }

    public static MarketNameChanged Handle(ChangeMarketNameCommand changeMarketName)
    {
        return new MarketNameChanged(changeMarketName.Id, changeMarketName.Name);
    }
}