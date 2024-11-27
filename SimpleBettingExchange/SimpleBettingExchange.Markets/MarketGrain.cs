using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace SimpleBettingExchange.Markets;

public interface IMarketGrain : IGrainWithGuidKey
{
    Task Create(Guid id, string name, MarketLineState[] lines);
}

public class MarketGrain : JournaledGrain<MarketState, IEvent>, IMarketGrain, ICustomStorageInterface<MarketState, IEvent>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<MarketGrain> _logger;
    private string _streamId;

    public MarketGrain(IEventStore eventStore, ILogger<MarketGrain> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamId = $"Market-{this.GetPrimaryKey()}";

        Console.WriteLine($"Welcome _streamId {_streamId}!!");

        return base.OnActivateAsync(cancellationToken);
    }

    public Task Create(Guid id, string name, MarketLineState[] lines)
    {
        var @event = new MarketCreated(id, name, lines, DateTimeOffset.Now);

        RaiseEvent(@event);
        return ConfirmEvents();
    }

    private async Task<IEvent[]> LoadEvents()
    {
        return await _eventStore.LoadStreamAsync(_streamId);
    }

    public async Task<KeyValuePair<int, MarketState>> ReadStateFromStorage()
    {
        _logger.LogInformation("Reading events for aggregate {0}", GrainReference.GetPrimaryKey());
        var root = new MarketState();

        var events = await LoadEvents();

        foreach(var @event in events)
        {
            root.When(@event);
        }

        return new KeyValuePair<int, MarketState>(0, root);
    }

    public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IEvent> updates, int expectedVersion)
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
        await _eventStore.AppendToStreamAsync(_streamId, updates);
        return true;
    }
}

