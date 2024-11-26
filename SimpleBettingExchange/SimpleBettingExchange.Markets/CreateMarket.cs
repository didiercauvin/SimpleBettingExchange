using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SimpleBettingExchange.Markets;

public record CreateMarketRequest(string Name, CreateMarketLineRequest[] Lines);
public record CreateMarketLineRequest(string Name);

public static class MarketEndPoints
{
    public static IEndpointRouteBuilder UseCreateMarketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/markets", async (CreateMarketRequest body, IMarketRepository repository) =>
        {
            var handler = new CreateMarketCommandHandler(repository);

            await handler.Handle(new CreateMarketCommand(body.Name, body.Lines.Select(l => new CreateMarketLine(Guid.NewGuid(), l.Name))), CancellationToken.None);
        });
        
        return endpoints;
    }
}

public record CreateMarketCommand(string Name, IEnumerable<CreateMarketLine> Lines);

public record CreateMarketLine(Guid Id, string Name);

public class CreateMarketCommandHandler
{
    private readonly IMarketRepository _repository;

    public CreateMarketCommandHandler(IMarketRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CreateMarketCommand command, CancellationToken ct)
    {
        var marketId = Guid.NewGuid();
        var (name, lines) = command;

        var market = Market.Create(marketId, name, lines.Select(l => new MarketLine(l.Id, l.Name)));

        await _repository.Add(market, ct);
    }
}