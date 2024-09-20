using JasperFx.Core;
using Marten;
using Marten.AspNetCore;
using Marten.Events.Aggregation;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Simple.BettingExchange.Api.Modules.Markets.MarketHandlers;

namespace Simple.BettingExchange.Api.Modules.Markets;


public record MarketLine(string Name);
public sealed record CreateMarket(string Name, MarketLine[] Lines);
public sealed record OpenMarket(Guid MarketId, DateTimeOffset OpenedAt);
public sealed record SuspendMarket(Guid MarketId, string Reason, DateTimeOffset SuspendedAt);
public sealed record ResumeMarket(Guid MarketId);
public sealed record CloseMarket(Guid MarketId);

public record MarketInitiated(Guid Id, string Name, MarketSelection[] Selections);
public record MarketOpened(Guid Id, DateTimeOffset Date);
public record MarketClosed(Guid Id, DateTimeOffset Date);
public record MarketSuspended(Guid Id, string Reason, DateTimeOffset Date);
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
        => new Market(initiated.Id, initiated.Name, initiated.Selections, MarketStatus.Initiated);

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
    {
        return new(initiated.Id, initiated.Name, initiated.Selections.Select(l => new ShortMarketSelection(l.Id, l.Name, l.OddsBack, l.OddsLay)).ToArray(), "Initié");
    }

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
        var @event = Handle(command);

        return (
            new CreationResponse($"api/markets/{@event.Id}"),
            new StartStream<Market>(@event.Id, @event)
       );
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/open")]
    public static (IResult, Events) OpenMarket(OpenMarket command, Market market)
    {
        var @event = Handle(command, market);

        return (Ok(), [@event]);
    }

    [AggregateHandler]
    [WolverinePost("/api/markets/{id}/suspend")]
    public static (IResult, Events) Suspend(SuspendMarket command, Market market)
    {
        var @event = Handle(command, market);

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

public static class MarketHandlers
{
    public static MarketInitiated Handle(CreateMarket command)
    {
        if (!command.Lines.Any())
        {
            throw new Exception("MARKET_CREATION_EMPTY_SELECTIONS");
        }

        return new MarketInitiated(Guid.NewGuid(), command.Name, command.Lines.Select(l => new MarketSelection(Guid.NewGuid(), l.Name)).ToArray());
    }

    public static MarketOpened Handle(OpenMarket command, Market market)
    {
        if (market.Status == MarketStatus.Opened)
        {
            throw new Exception("MARKET_CANNOT_BE_REOPEN");
        }

        return new MarketOpened(market.Id, command.OpenedAt);
    }

    public static MarketSuspended Handle(SuspendMarket command, Market market)
    {
        if (market.Status == MarketStatus.Initiated)
        {
            throw new Exception("INITIATED_MARKET_CANNOT_BE_SUSPENDED");
        }

        if (market.Status == MarketStatus.Closed)
        {
            throw new Exception("CLOSED_MARKET_CANNOT_BE_SUSPENDED");
        }

        return new MarketSuspended(market.Id, command.Reason, command.SuspendedAt);
    }
}