using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections;

namespace SimpleBettingExchange.Markets;

public enum MarketStatus {Created, Opened, Suspended, Closed}


public class Market
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public MarketStatus State { get; private set; }
    public MarketLines Lines { get; private set; }

    private Market(Guid id, string name, IEnumerable<MarketLine> lines)
    {
        Id = id;
        Name = name;
        State = MarketStatus.Created;
        Lines = new MarketLines(lines);
    }
    
    public static Market Create(Guid id, string name, IEnumerable<MarketLine> lines) 
        => new(id, name, lines);
}

public class MarketLine
{
    public MarketLine(Guid Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
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
