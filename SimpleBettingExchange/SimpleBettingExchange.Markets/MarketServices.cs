using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    public static MarketCreated Handle(CreateMarket createMarket)
    {
        var (id, name, lines) = createMarket;
        return new MarketCreated(id, name, lines.Select(l => new MarketLineState(l.Id, l.Name)).ToArray(), DateTimeOffset.Now);
    }

    public static MarketNameChanged Handle(ChangeMarketName changeMarketName)
    {
        return new MarketNameChanged(changeMarketName.Id, changeMarketName.Name);
    }
}