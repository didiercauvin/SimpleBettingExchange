using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    

    

    

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