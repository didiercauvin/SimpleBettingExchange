using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public record ChangeMarketNameRequest(string Name);

public record ChangeMarketName(Guid Id, string Name);

public static class ChangeMarketNameEndPoint
{
    public static IEndpointRouteBuilder UseChangeMarketNameEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets/{marketId:Guid}", async (Guid marketId, ChangeMarketNameRequest request, IMarketRepository repository, CancellationToken ct) =>
        {
            await MarketServices.Handle(new ChangeMarketName(marketId, request.Name), repository, ct);
        });

        return endpoints;
    }
}
