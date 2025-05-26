using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Wolverine.Http;

namespace SimpleBettingExchange.Markets;

public record GetAllMarketsResponse(GetAllMarketItem[] Markets);

public record GetAllMarketItem(Guid Id, string Name, DateTimeOffset StartTime);

public class GetAllMarkets
{
    [WolverineGet("/api/markets")]
    public static async Task<GetAllMarketsResponse> Get(IDocumentSession session)
    {
        var markets = await session.Query<Market>().ToListAsync();
        return new GetAllMarketsResponse(markets.Select(m => new GetAllMarketItem(m.Id, m.Name, DateTimeOffset.Now))
            .ToArray());
    }
}
