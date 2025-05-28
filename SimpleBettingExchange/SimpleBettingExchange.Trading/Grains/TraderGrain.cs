using Orleans;

namespace SimpleBettingExchange.Trading.Grains;

public interface ITraderGrain
{
    
}

public class TraderGrain : Grain<TraderState>, ITraderGrain
{
    
}

public class TraderState
{
    
}