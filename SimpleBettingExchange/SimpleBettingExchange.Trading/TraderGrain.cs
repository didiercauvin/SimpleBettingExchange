using Marten;

namespace SimpleBettingExchange.Trading;

public interface ITraderGrain : IGrainWithGuidKey
{
}

public record BackStateSubmitted();

public class TraderState
{
    
}

public class TraderGrain : Grain<TraderState>, ITraderGrain
{
    private readonly IDocumentSession _documentSession;

    private Guid _streamId;
    private TraderState _state = new();
    
    public TraderGrain(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamId = this.GetPrimaryKey();
        
        var trader = await _documentSession.Events.AggregateStreamAsync<Trader>(_streamId);
        _state = trader is not null ? trader.ToState() : Trader.None.ToState();
        
        await base.OnActivateAsync(cancellationToken);
    }
}

public static class TraderExtensions
{
    public static TraderState ToState(this Trader trader)
    {
        return new TraderState();
    }

    public static Trader ToTrader(this TraderState state)
    {
        return new Trader();
    }
}