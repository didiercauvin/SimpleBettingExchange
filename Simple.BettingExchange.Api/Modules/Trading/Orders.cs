using JasperFx.Core;
using Marten;
using Marten.Events.Aggregation;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Simple.BettingExchange.Api.Modules.Trading;

public sealed record PlaceOrder(Guid MarketId, Guid TraderId, Guid SelectionId, string OrderType, decimal Odds, decimal Price);
public sealed record MatchOrder(Guid OrderId);
public sealed record RefuseOrder(Guid OrderId, string Reason);
public sealed record CancelOrder(Guid OrderId);
public sealed record SettleOrder(Guid OrderId);
public sealed record LapseOrder(Guid OrderId);

public record OrderPended(Guid MarketId, Guid OrderId, Guid TraderId, Guid SelectionId, OrderType OrderType, decimal Odds, decimal Price);
public record OrderMatched(Guid OrderId);
public record OrderRefused(Guid OrderId, string Reason);
public record OrderCancelled(Guid OrderId);
public record OrderSettled(Guid OrderId);
public record OrderLapsed(Guid OrderId);

public enum OrderType
{
    Back,
    Lay
}

public enum OrderStatus
{
    Active,
    Pended,
    Refused,
    Cancelled,
    Settled,
    Lapsed
}

public sealed record Order(Guid Id, Guid MarketId, Guid TraderId, Guid SelectionId, OrderType OrderType, decimal Odds, decimal Price, OrderStatus Status = OrderStatus.Pended)
{
    public static Order Create(OrderPended pended)
    {
        var (marketId, orderId, traderId, selectionId, orderType, odds, price) = pended;

        return new Order(orderId, marketId, traderId, selectionId, orderType, odds, price);
    }

    public Order Apply(OrderMatched matched)
        => this with
        {
            Status = OrderStatus.Active
        };

    public Order Apply(OrderRefused refused)
        => this with
        {
            Status = OrderStatus.Refused
        };

    public Order Apply(OrderCancelled cancelled)
        => this with
        {
            Status = OrderStatus.Cancelled
        };

    public Order Apply(OrderLapsed lapsed)
        => this with
        {
            Status = OrderStatus.Lapsed
        };

    public Order Apply(OrderSettled settled)
        => this with
        {
            Status = OrderStatus.Settled
        };

}

public record OrderResume(Guid Id, Guid MarketId, Guid TraderId, Guid SelectionId, string OrderType, decimal Odds, decimal Price, string Status = "En attente");

public sealed class AllMarketOrdersProjection : SingleStreamProjection<OrderResume>
{
    public static OrderResume Create(OrderPended pended)
    {
        var (marketId, orderId, traderId, selectionId, orderType, odds, price) = pended;

        return new(orderId, marketId, traderId, selectionId, orderType.ToString(), odds, price);
    }

    public OrderResume Apply(OrderMatched matched, OrderResume order)
    => order with
    {
        Status = "En cours"
    };

    public OrderResume Apply(OrderRefused refused, OrderResume order)
    => order with
    {
        Status = refused.Reason
    };

    public OrderResume Apply(OrderCancelled cancelled, OrderResume order)
    => order with
    {
        Status = "Annulé"
    };

    public OrderResume Apply(OrderSettled settled, OrderResume order)
    => order with
    {
        Status = "Payé"
    };

    public OrderResume Apply(OrderLapsed lapsed, OrderResume order)
    => order with
    {
        Status = "Expiré"
    };
}

public static class TradingEndpointHandler
{
    [WolverineGet("/api/orders/{marketId}")]
    public static Task<IReadOnlyList<OrderResume>> GetMarkets(IQuerySession querySession, CancellationToken ct) =>
        querySession.Query<OrderResume>().ToListAsync();

    [WolverinePost("/api/orders/place")]
    public static (CreationResponse, IStartStream) PlaceOrder(PlaceOrder command)
    {
        var (marketId, traderId, selectionId, orderType, odds, price) = command;

        var orderId = CombGuidIdGeneration.NewGuid();
        var @event = new OrderPended(marketId, orderId, traderId, selectionId, (OrderType)Enum.Parse(typeof(OrderType), orderType), odds, price);
        return (
        new CreationResponse($"api/orders/{orderId}"),
            new StartStream<Order>(orderId, @event)
        );
    }

    [AggregateHandler]
    [WolverinePost("/api/orders/{id}/match")]
    public static (IResult, Events) Match(MatchOrder command, Order order)
    {
        return (
                Ok(),
                [new OrderMatched(order.Id)]
            );
    }

    [AggregateHandler]
    [WolverinePost("/api/orders/{id}/refuse")]
    public static (IResult, Events) Refuse(RefuseOrder command, Order order)
    {
        return (
                Ok(),
                [new OrderRefused(order.Id, command.Reason)]
            );
    }

    [AggregateHandler]
    [WolverinePost("/api/orders/{id}/cancel")]
    public static (IResult, Events) Cancel(CancelOrder command, Order order)
    {
        return (
                Ok(),
                [new OrderCancelled(order.Id)]
            );
    }

    [AggregateHandler]
    [WolverinePost("/api/orders/{id}/lapse")]
    public static (IResult, Events) Lapse(LapseOrder command, Order order)
    {
        return (
                Ok(),
                [new OrderLapsed(order.Id)]
            );
    }

    [AggregateHandler]
    [WolverinePost("/api/orders/{id}/settle")]
    public static (IResult, Events) Settle(SettleOrder command, Order order)
    {
        return (
                Ok(),
                [new OrderSettled(order.Id)]
            );
    }
}

