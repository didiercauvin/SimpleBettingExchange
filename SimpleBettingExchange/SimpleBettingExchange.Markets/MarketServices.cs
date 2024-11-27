using Microsoft.CodeAnalysis;

namespace SimpleBettingExchange.Markets;

public static class MarketServices
{
    public static async Task Handle(CreateMarket createMarket, IMarketRepository repository, CancellationToken ct)
    {
        var market = Market.Create(createMarket.Id, createMarket.Name, createMarket.Lines.Select(l => new MarketLine(Guid.NewGuid(), l.Name)));

        await repository.Add(market, ct);
    }
}