namespace SimpleBettingExchange.Markets;

[GenerateSerializer]
public class MarketState
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public string Name { get; set; }
    [Id(2)]
    public MarketStatus Status { get; set; }
    [Id(3)]
    public DateTimeOffset StartTime { get; set; }
    [Id(4)]
    public DateTimeOffset? EndTime { get; set; }
    [Id(5)]
    public RunnerState[] Lines { get; set; } = [];
}

[GenerateSerializer]
public class RunnerState(Guid id, string name, PriceState[] backPrices, PriceState[] layPrices)
{
    [Id(0)] public Guid Id { get; } = id;
    [Id(1)] public string Name { get; } = name;

    [Id(2)] public PriceState[] BackPrices { get;  } = backPrices;
    [Id(3)] public PriceState[] LayPrices { get; set; } = layPrices;
}

[GenerateSerializer]
public record PriceState(decimal PriceValue, decimal Size);