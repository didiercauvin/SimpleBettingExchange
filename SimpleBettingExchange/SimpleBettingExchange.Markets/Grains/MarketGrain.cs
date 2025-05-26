using Marten;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;

namespace SimpleBettingExchange.Markets;

public interface IMarketGrain : IGrainWithGuidKey
{
    Task<MarketCreated> CreateMarket(string name, DateTimeOffset startTime);
    Task ChangeName(ChangeMarketNameCommand command);
    Task AddRunners(AddRunnersCommand command);
    Task SuspendMarket(SuspendMarketCommand command);
    Task ResumeMarket(ResumeMarketCommand command);
    Task CloseMarket(CloseMarketCommand command);
    Task<Market> GetMarketState();
}

public class MarketGrain : Grain<Market>, IMarketGrain
{
    private readonly IDocumentSession _documentSession;
    private readonly ILogger<MarketGrain> _logger;
    private Guid _streamId;
    private Market _state = new();

    public MarketGrain(IDocumentSession documentSession, ILogger<MarketGrain> logger)
    {
        this._documentSession = documentSession;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamId = this.GetPrimaryKey();
        
        _state = await _documentSession.Events.AggregateStreamAsync<Market>(_streamId);
        
        // var stream = await _querySession.Events.FetchStreamAsync(streamId);
        // var events = await _querySession.Events.FetchStreamAsync(stream);
        //
        // foreach (var e in events)
        // {
        //     Apply(e.Data);
        // }

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<MarketCreated> CreateMarket(string name, DateTimeOffset startTime)
    {
        var @event = new MarketCreated(this.GetPrimaryKey(), name, startTime, DateTimeOffset.UtcNow);
        _documentSession.Events.StartStream<Market>(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.None;
        _state.Apply(@event);
        
        return @event;
    }

    public Task AddRunners(AddRunnersCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task SuspendMarket(SuspendMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task ResumeMarket(ResumeMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task CloseMarket(CloseMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task ChangeName(ChangeMarketNameCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task<Market> GetMarketState()
    {
        return Task.FromResult(_state);
    }
}

