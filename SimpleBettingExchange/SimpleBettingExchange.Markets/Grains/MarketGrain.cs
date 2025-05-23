using Marten;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace SimpleBettingExchange.Markets;

public interface IMarketGrain : IGrainWithGuidKey
{
    Task Handle(MarketCreated @event);
    Task CreateMarket(CreateMarketCommand command);
    Task ChangeName(ChangeMarketNameCommand command);
    Task AddRunners(AddRunnersCommand command);
    Task SuspendMarket(SuspendMarketCommand command);
    Task ResumeMarket(ResumeMarketCommand command);
    Task CloseMarket(CloseMarketCommand command);
    Task<MarketState> GetMarketState();
}

public class MarketGrain : Grain<MarketState>, IMarketGrain
{
    private readonly IQuerySession _querySession;
    private readonly ILogger<MarketGrain> _logger;
    private string _streamId;
    private MarketState _state = new();

    public MarketGrain(IQuerySession querySession, ILogger<MarketGrain> logger)
    {
        _querySession = querySession;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamId = this.GetPrimaryKey();
        var events = await _querySession.Events.FetchStreamAsync(streamId);

        foreach (var e in events)
        {
            Apply(e.Data);
        }

        await base.OnActivateAsync(cancellationToken);
    }

    private void Apply(object @event)
    {
        switch (@event)
        {
            case MarketCreated created:
                _state.Name = created.Name;
                _state.StartTime = created.StartTime;
                break;
                // autres événements...
        }
    }

    public Task Handle(MarketCreated @event)
    {
        Apply(@event);
        return WriteStateAsync();
    }

    public Task CreateMarket(CreateMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
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

    private Task<IEvent[]> LoadEvents()
    {
        return Task.FromResult(Array.Empty<IEvent>());
        //return await _eventStore.LoadStreamAsync(_streamId);
    }

    public async Task<KeyValuePair<int, MarketState>> ReadStateFromStorage()
    {
        _logger.LogInformation("Reading events for aggregate {0}", GrainReference.GetPrimaryKey());
        var root = new MarketState();

        var events = await LoadEvents();

        foreach (var @event in events)
        {
            root.When(@event);
        }

        return new KeyValuePair<int, MarketState>(0, root);
    }

    public Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IEvent> updates, int expectedVersion)
    {
        _logger.LogInformation("Applying Events for Aggregate {0}", GrainReference.GetPrimaryKey());
        //var version = await GetCurrentVersion();
        //if (version != expectedVersion)
        //{
        //    _logger.LogCritical("Expected version not matched for {0} ==> {1}!= {2}",
        //        GrainReference.GetPrimaryKey().ToString("N"), version, expectedVersion);
        //    throw new AccountTransactionException(
        //        $"Concurrency Exception Detected!");
        //}
        //await _eventStore.AppendToStreamAsync(_streamId, updates);
        return Task.FromResult(true);
    }

    public Task<MarketState> GetMarketState()
    {
        return Task.FromResult(State);
    }
}

