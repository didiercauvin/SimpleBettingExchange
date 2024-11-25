using System.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public enum MarketState {Created, Opened, Suspended, Closed}

public record MarketCreated(Guid Id, string Name, IEnumerable<MarketLine> Lines, DateTimeOffset CreatedAt);

public class Market : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public MarketState State { get; private set; }
    public MarketLines Lines { get; private set; }

    private Market(Guid id, string name, IEnumerable<MarketLine> lines)
    {
        Apply(new MarketCreated(id, name, lines, DateTimeOffset.Now));
    }
    
    public static Market Create(Guid id, string name, IEnumerable<MarketLine> lines) 
        => new(id, name, lines);

    protected override void When(object @event)
    {
        switch (@event)
        {
            case MarketCreated created:
                Id = created.Id;
                Name = created.Name;
                State = MarketState.Created;
                Lines = new MarketLines(created.Lines);
                break;
            default: throw new ArgumentOutOfRangeException($"Event {nameof(@event)} does not exists");
        };
    }
}

[GenerateSerializer]
public class MarketLine
{
    public MarketLine(Guid Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }
    
    [Id(0)] public Guid Id { get; private set; }
    [Id(1)] public string Name { get; private set; }
}

public class MarketLines : IEnumerable<MarketLine>
{
    private readonly List<MarketLine> _lines = new List<MarketLine>();

    public MarketLines(IEnumerable<MarketLine> lines)
    {
        _lines.AddRange(lines);
    }
    
    public IEnumerator<MarketLine> GetEnumerator()
    {
        foreach (var line in _lines)
        {
            yield return line;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
