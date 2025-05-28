using SimpleBettingEchange.Core;

namespace SimpleBettingExchange.Trading;

public class Trader
{
    public static Trader None => new();
    
    public static Trader When(Trader state, IEvent @event)
    {
        return @event switch
        {
            _ => throw new ArgumentException("Unknown type of event", nameof(@event))
        };
    }

}