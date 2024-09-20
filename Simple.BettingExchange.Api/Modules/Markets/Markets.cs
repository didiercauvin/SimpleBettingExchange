using JasperFx.Core;
using Marten;
using Marten.AspNetCore;
using Marten.Events.Aggregation;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Simple.BettingExchange.Api.Modules.Markets;


public record MarketLine(string Name);
public sealed record CreateMarket(string Name, MarketLine[] Lines);
public sealed record OpenMarket(Guid MarketId);
public sealed record SuspendMarket(Guid MarketId);
public sealed record ResumeMarket(Guid MarketId);
public sealed record CloseMarket(Guid MarketId);

public record MarketInitiated(Guid Id, string Name, MarketSelection[] Lines);
public record MarketOpened(Guid Id, DateTimeOffset Date);
public record MarketClosed(Guid Id, DateTimeOffset Date);
public record MarketSuspended(Guid Id, DateTimeOffset Date);
public record MarketResumed(Guid Id, DateTimeOffset Date);

public enum MarketStatus
{
    Initiated,
    Opened,
    Suspended,
    Closed
}

public record MarketSelection(Guid Id, string Name, decimal OddsBack = 5, decimal OddsLay = 5);

public record Market(Guid Id, string Name, MarketSelection[] Lines, MarketStatus Status)
{

    public static Market Create(MarketInitiated initiated)
        => new Market(initiated.Id, initiated.Name, initiated.Lines, MarketStatus.Initiated);

    public Market Apply(MarketOpened opened)
    => this with
    {
        Status = MarketStatus.Opened
    };

    public Market Apply(MarketSuspended suspended)
    => this with
    {
        Status = MarketStatus.Suspended
    };

    public Market Apply(MarketResumed resumed)
    => this with
    {
        Status = MarketStatus.Opened
    };

    public Market Apply(MarketClosed closed)
    => this with
    {
        Status = MarketStatus.Closed
    };
}

public sealed record ShortMarket(Guid Id, string Name, ShortMarketSelection[] Selections, string Status);
public sealed record ShortMarketSelection(Guid Id, string Name, decimal OddsBack, decimal OddsLay);

public sealed class AllMarketsProjection : SingleStreamProjection<ShortMarket>
{
    public static ShortMarket Create(MarketInitiated initiated) 
        => new(initiated.Id, initiated.Name, initiated.Lines.Select(l => new ShortMarketSelection(l.Id, l.Name, l.OddsBack, l.OddsLay)).ToArray(), "Initié");

    public ShortMarket Apply(MarketOpened opened, ShortMarket market)
        => market with
        {
            Status = "Ouvert"
        };

    public ShortMarket Apply(MarketSuspended suspended, ShortMarket market)
        => market with
        {
            Status = "Suspendu"
        };

    public static ShortMarket Apply(MarketResumed resumed, ShortMarket market)
        => market with
        {
            Status = "Ouvert"
        };

    public ShortMarket Apply(MarketClosed closed, ShortMarket market)
        => market with
        {
            Status = "Fermé"
        };
}

public static class MarketEndPointHandler
{
    [WolverineGet("/api/markets")]
    public static Task<IReadOnlyList<ShortMarket>> GetMarkets(IQuerySession querySession, CancellationToken ct) =>
        querySession.Query<ShortMarket>().ToListAsync();

    [WolverineGet("/api/markets/{id}")]
    public static Task GetMarket([FromRoute] Guid id, IQuerySession querySession, HttpContext context) =>
        querySession.Json.WriteById<ShortMarket>(id, context);

    [WolverineGet("/api/markets/{id}/events")]
    public static async Task<dynamic> GetAllEventsForMarket([FromRoute] Guid id, IQuerySession session, CancellationToken ct)
    {
        var events = await session.Events.FetchStreamAsync(id);

        return events.Select(e => new { Evenement = e.Data.ToString() });
    }

    [WolverinePost("/api/markets")]
    public static (CreationResponse, IStartStream) OpenMarket(CreateMarket command)
    {
        var marketId = CombGuidIdGeneration.NewGuid();
        var @event = new MarketInitiated(marketId, command.Name, command.Lines.Select(l => new MarketSelection(Guid.NewGuid(), l.Name)).ToArray());

        return (
            new CreationResponse($"api/markets/{marketId}"),
            new StartStream<Market>(marketId, @event)
       );
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/open")]
    public static (IResult, Events) OpenMarket(OpenMarket command, Market market)
    {
        var @event = new MarketOpened(market.Id, DateTimeOffset.Now);

        return (Ok(), [@event]);
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/suspend")]
    public static (IResult, Events) Suspend(SuspendMarket command, Market market)
    {
        var @event = new MarketSuspended(market.Id, DateTimeOffset.Now);

        return (Ok(), [@event]);
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/resume")]
    public static (IResult, Events) Resume(ResumeMarket command, Market market)
    {
        var @event = new MarketResumed(market.Id, DateTimeOffset.Now);

        return (Ok(), [@event]);
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/close")]
    public static (IResult, Events) Close(CloseMarket command, Market market)
    {
        var @event = new MarketClosed(market.Id, DateTimeOffset.Now);

        return (Ok(), [@event]);
    }
}