namespace SimpleBettingExchange.Markets;

public class Market
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MarketStatus Status { get; set; }
    public MarketLine[] Lines { get; set; }

    private Market(Guid id, string name, IEnumerable<MarketLine> lines)
    {
        Id = id;
        Name = name;
        Status = MarketStatus.Created;
        Lines = lines.ToArray();
    }

    public static Market Create(Guid id, string name, IEnumerable<MarketLine> lines)
        => new Market(id, name, lines);
}

public enum MarketStatus { Created, Opened, Suspended, Closed }

public class MarketLine
{
    public MarketLine(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
}
