using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ogooreck.BusinessLogic;
using Simple.BettingExchange.Api.Modules.Markets;
using System.Linq;
using static Simple.BettingExchange.Api.Modules.Markets.MarketHandlers;

namespace Simple.BettingExchange.Tests.Modules.Markets;

[TestClass]
public class MarketTests
{
    private HandlerSpecification<Market> _spec;

    private static readonly Func<Market, object, Market> evolve =
        (market, @event) =>
        {
            return @event switch
            {
                MarketInitiated initiated => Market.Create(initiated),
                MarketOpened opened => market.Apply(opened),
                MarketSuspended suspended => market.Apply(suspended),
                MarketClosed closed => market.Apply(closed),
                _ => market
            };
        };

    [TestInitialize]
    public void Setup()
    {
        _spec = Specification.For<Market>(evolve);
    }

    [TestMethod]
    public void market_cannot_have_empty_selections()
    {
        _spec.Given()
            .When(() => Handle(new CreateMarket("marché", Array.Empty<MarketLine>())))
            .ThenThrows<Exception>(exception => exception.Message.Should().Be("MARKET_CREATION_EMPTY_SELECTIONS"));
    }

    [TestMethod]
    public void market_with_selections_can_be_created()
    {
        var marketId = Guid.NewGuid();

        _spec.Given()
            .When(() => Handle(new CreateMarket("marché", new MarketLine[] { new("selection") })))
            .Then(events =>
            {
                var initiated = events.Single() as MarketInitiated;
                initiated!.Id.Should().NotBeEmpty();
                initiated.Name.Should().Be("marché");
                initiated.Selections.Should().HaveCount(1);
            });
    }

    [TestMethod]
    public void initiated_market_can_be_opened()
    {
        var marketId = Guid.NewGuid();
        var initiated = new MarketInitiated(marketId, "marché", Array.Empty<MarketSelection>());

        _spec.Given(initiated)
             .When(market => Handle(new OpenMarket(marketId, DateTimeOffset.Parse("20/09/2024 14:00:00")), market))
             .Then(new MarketOpened(marketId, DateTimeOffset.Parse("20/09/2024 14:00:00")));
    }

    [TestMethod]
    public void already_opened_market_cannot_be_open_again()
    {
        var marketId = Guid.NewGuid();
        var opened = new MarketOpened(marketId, DateTimeOffset.Parse("20/09/2024 14:00:00"));

        _spec.Given(opened)
            .When(market => Handle(new OpenMarket(marketId, DateTimeOffset.Parse("20/09/2024 15:00:00")), market))
            .ThenThrows<Exception>(exception => exception.Message.Should().Be("MARKET_CANNOT_BE_REOPEN"));
    }

    [TestMethod]
    public void opened_market_can_be_suspended()
    {
        var marketId = Guid.NewGuid();
        var initiated = new MarketInitiated(marketId, "marché", new MarketSelection[] { new(Guid.NewGuid(), "selection") });
        var opened = new MarketOpened(marketId, DateTimeOffset.Parse("20/09/2024 14:00:00"));

        _spec.Given(
                initiated,
                opened
             )
            .When(market => Handle(new SuspendMarket(marketId, "trop de pluie", DateTimeOffset.Parse("20/09/2024 18:00:00")), market))
            .Then(new MarketSuspended(marketId, "trop de pluie", DateTimeOffset.Parse("20/09/2024 18:00:00")));
    }

    [TestMethod]
    public void initiated_market_cannot_be_suspended()
    {
        var marketId = Guid.NewGuid();
        var initiated = new MarketInitiated(marketId, "marché", new MarketSelection[] { new(Guid.NewGuid(), "selection") });

        _spec.Given(
                initiated
             )
            .When(market => Handle(new SuspendMarket(marketId, "trop de pluie", DateTimeOffset.Parse("20/09/2024 18:00:00")), market))
            .ThenThrows<Exception>(exception => exception.Message.Should().Be("INITIATED_MARKET_CANNOT_BE_SUSPENDED"));
    }

    [TestMethod]
    public void closed_market_cannot_be_suspended()
    {
        var marketId = Guid.NewGuid();
        var initiated = new MarketInitiated(marketId, "marché", new MarketSelection[] { new(Guid.NewGuid(), "selection") });
        var closed = new MarketClosed(marketId, DateTimeOffset.Parse("20/09/2024 17:00:00"));

        _spec.Given(
                initiated,
                closed
             )
            .When(market => Handle(new SuspendMarket(marketId, "trop de pluie", DateTimeOffset.Parse("20/09/2024 18:00:00")), market))
            .ThenThrows<Exception>(exception => exception.Message.Should().Be("CLOSED_MARKET_CANNOT_BE_SUSPENDED"));
    }
}