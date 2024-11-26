namespace SimpleBettingExchange.Markets;

public interface IEvent
{
    
}

public interface IEventStore
{
    Task AppendToStreamAsync(string streamId, IEnumerable<IEvent> events);
    Task<IEvent[]> LoadStreamAsync(string streamId);
}

public class InMemoryEventStore : IEventStore
{
    private Dictionary<string, List<IEvent>> _streams = new();

    public Task AppendToStreamAsync(string streamId, IEnumerable<IEvent> events)
    {
        if (!_streams.ContainsKey(streamId))
        {
            _streams.Add(streamId, events.ToList());
        }
        else
        {
            _streams[streamId].AddRange(events);
        }

        return Task.CompletedTask;
    }

    public Task<IEvent[]> LoadStreamAsync(string streamId)
    {
        return Task.FromResult(_streams.ContainsKey(streamId) ? _streams[streamId].ToArray() : Array.Empty<IEvent>());
    }
}