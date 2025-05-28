using JasperFx.Core;
using Marten;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;

namespace SimpleBettingExchange.Markets;

public interface IMarketGrain : IGrainWithGuidKey
{
    Task Handle(MarketStateCreated @event);
    Task Handle(MarketStateNameChanged @event);
    Task Handle(MarketStateRunnersAdded @event);
    Task SuspendMarket(SuspendMarketCommand command);
    Task ResumeMarket(ResumeMarketCommand command);
    Task CloseMarket(CloseMarketCommand command);
    Task<MarketState> GetMarketState();
}

public class MarketGrain : Grain<MarketState>, IMarketGrain
{
    private readonly IDocumentSession _documentSession;
    private readonly ILogger<MarketGrain> _logger;
    private Guid _streamId;
    private MarketState _state = new();

    public MarketGrain(IDocumentSession documentSession, ILogger<MarketGrain> logger)
    {
        this._documentSession = documentSession;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamId = this.GetPrimaryKey();
        
        var market = await _documentSession.Events.AggregateStreamAsync<Market>(_streamId);
        _state = market is not null ? market.ToState() : Market.None.ToState();
        
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task Handle(MarketStateCreated created)
    {
        var @event = new MarketCreated(created.Id, created.EventName, created.Name, created.StartTime, created.CreatedAt);
        _documentSession.Events.StartStream<Market>(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(Market.None, @event).ToState();
    }

    public async Task Handle(MarketStateNameChanged changed)
    {
        var @event = new MarketNameChanged(changed.Id, changed.Name);
        _documentSession.Events.Append(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(_state.ToMarket(), @event).ToState();
    }

    public async Task Handle(MarketStateRunnersAdded runnersAdded)
    {
        var @event = new MarketRunnersAdded(
            runnersAdded.MarketId, 
            runnersAdded.Runners.Select(r => new RunnerSnapshot(
                r.RunnerId,
                r.Name,
                new PriceSnapshot(r.BackPrice.Price, r.BackPrice.Size),
                new PriceSnapshot(r.LayPrice.Price, r.LayPrice.Size))).ToArray());
        
        _documentSession.Events.Append(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(_state.ToMarket(), @event).ToState();
    }

    public Task SuspendMarket(SuspendMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task ResumeMarket(ResumeMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task CloseMarket(CloseMarketCommand command)
    {
        //var @event = MarketServices.Handle(command);

        //RaiseEvent(@event);
        //return ConfirmEvents();

        return Task.CompletedTask;
    }

    public Task<MarketState> GetMarketState()
    {
        return Task.FromResult(_state);
    }
}

public static class MarketExtensions
{
    public static MarketState ToState(this Market market)
        => new MarketState
        {
            Id = market.Id,
            EventName = market.EventName,
            Name = market.Name,
            StartTime = market.StartTime,
            EndTime = market.EndTime,
            Status = market.Status,
            Lines = market.Lines.Select(l => 
                new RunnerState(
                    l.Id, 
                    l.Name, 
                    l.BackPrices.Select(p => new PriceState(p.PriceValue, p.Size)).ToArray(), 
                    l.LayPrices.Select(p => new PriceState(p.PriceValue, p.Size)).ToArray()
                )
            ).ToArray(),
        };
}

public static class MarketStateExtensions
{
    public static Market ToMarket(this MarketState market)
        => new Market
        {
            Id = market.Id,
            EventName = market.EventName,
            Name = market.Name,
            StartTime = market.StartTime,
            EndTime = market.EndTime,
            Status = market.Status,
            Lines = market.Lines.Select(l => 
                new Runner(
                    l.Id, 
                    l.Name, 
                    l.BackPrices.Select(p => new Price(p.PriceValue, p.Size)).ToArray(), 
                    l.LayPrices.Select(p => new Price(p.PriceValue, p.Size)).ToArray()
                )
            ).ToArray(),
        };
}

