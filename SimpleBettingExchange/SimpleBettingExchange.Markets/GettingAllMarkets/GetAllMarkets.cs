using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Marten.Events.Projections;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record GetAllMarketsResponse(GetAllMarketItem[] Markets);

public record GetAllMarketItem(Guid Id, string Name, DateTimeOffset StartTime);

public class GetAllMarkets
{
    [WolverineGet("/api/markets")]
    public static async Task<GetAllMarketsResponse> Get(IDocumentSession session)
    {
        var markets = await session.Query<MarketSummary>().ToListAsync();
        return new GetAllMarketsResponse(markets.Select(m => new GetAllMarketItem(m.Id, m.Name, DateTimeOffset.Now))
            .ToArray());
    }
}

public class MarketListProjection : MultiStreamProjection<MarketSummary, Guid>
{
    public MarketListProjection()
    {
        Identity<MarketCreated>(x => x.Id);
        // Identity<MarketUpdated>(x => x.MarketId);
        // Identity<MarketDeactivated>(x => x.MarketId);
    }

    public MarketSummary Create(MarketCreated created)
    {
        return new MarketSummary
        {
            Id = created.Id,
            Name = created.Name,
            Status = "Created",
            StartTime = created.StartTime,
            CreatedAt = created.CreatedAt,
        };
    }

    // public MarketSummary Apply(MarketUpdated updated, MarketSummary current)
    // {
    //     current.Name = updated.Name;
    //     current.UpdatedAt = updated.Timestamp;
    //     return current;
    // }

    // public void Apply(MarketDeactivated deactivated, MarketSummary current)
    // {
    //     current.Status = "Inactive";
    //     current.UpdatedAt = deactivated.Timestamp;
    // }
}

public class MarketSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
