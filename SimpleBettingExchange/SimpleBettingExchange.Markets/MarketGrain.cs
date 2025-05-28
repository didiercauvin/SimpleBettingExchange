using JasperFx.Core;
using Marten;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using SimpleBettingEchange.Core;

namespace SimpleBettingExchange.Markets;

[GenerateSerializer]
public record ChangeMarketNameCommand(Guid Id, string Name);

[GenerateSerializer]
public record AddRunnersCommand(Guid MarketId, AddRunnerItemCommand[] Runners);
[GenerateSerializer]
public record AddRunnerItemCommand(string Name, AddRunnerPriceCommand BackPrice, AddRunnerPriceCommand LayPrice);
[GenerateSerializer]
public record AddRunnerPriceCommand(decimal Price, decimal Size);

[GenerateSerializer]
public record SubmitBackBetCommand(Guid TraderId, decimal Odds, decimal Stake);

public interface IMarketGrain : IGrainWithGuidKey
{
    Task<MarketCreated> Handle(CreateMarketCommand command);
    Task<MarketNameChanged> Handle(ChangeMarketNameCommand command);
    Task<MarketRunnersAdded> Handle(AddRunnersCommand command);
    Task<IBackBetEvent> PlaceBackBet(SubmitBackBetCommand command);
    Task<IBackBetEvent> PlaceLayBet(Guid traderId, Guid selectionId, decimal odds, decimal stake);
    // Task SuspendMarket(SuspendMarketCommand command);
    // Task ResumeMarket(ResumeMarketCommand command);
    // Task CloseMarket(CloseMarketCommand command);
    Task<Market> GetMarketState();
}

[GenerateSerializer]
public record BetStatePlacementResult(bool IsAccepted, string? Reason = null);

public class MarketGrain : Grain<Market>, IMarketGrain
{
    private readonly IDocumentSession _documentSession;
    private readonly ILogger<MarketGrain> _logger;
    private Guid _streamId;
    private Market _state = new();

    public MarketGrain(IDocumentSession documentSession, ILogger<MarketGrain> logger)
    {
        this._documentSession = documentSession;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamId = this.GetPrimaryKey();
        
        var market = await _documentSession.Events.AggregateStreamAsync<Market>(_streamId);
        _state = market is not null ? market : Market.None;
        
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<MarketCreated> Handle(CreateMarketCommand command)
    {
        var @event = Handle(_streamId, command);
        _documentSession.Events.StartStream<Market>(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(Market.None, @event);

        return @event;
    }

    public async Task<MarketNameChanged> Handle(ChangeMarketNameCommand command)
    {
        var @event = Handle(_state, command);
        _documentSession.Events.Append(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(_state, @event);
        
        return @event;
    }

    public async Task<MarketRunnersAdded> Handle(AddRunnersCommand command)
    {
        var @event = Handle(_state, command);
        
        _documentSession.Events.Append(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(_state, @event);
        
        return @event;
    }

    public async Task<IBackBetEvent> PlaceBackBet(SubmitBackBetCommand command)
    {
        var @event = Handle(_state, command);
        
        _documentSession.Events.Append(_streamId, @event);
        await _documentSession.SaveChangesAsync();

        _state = Market.When(_state, @event);

        return @event;
    }

    public Task<IBackBetEvent> PlaceLayBet(Guid traderId, Guid selectionId, decimal odds, decimal stake)
    {
        throw new NotImplementedException();
    }

    public Task<Market> GetMarketState()
    {
        return Task.FromResult(_state);
    }
    
    private static MarketCreated Handle(Guid id, CreateMarketCommand createMarket)
    {
        var (eventName, name, lines) = createMarket;
        return new MarketCreated(id, eventName, name, createMarket.StartTime, DateTimeOffset.Now);
    }

    private static MarketNameChanged Handle(Market market, ChangeMarketNameCommand changeMarketName)
    {
        return new MarketNameChanged(changeMarketName.Id, changeMarketName.Name);
    }
    
    private static MarketRunnersAdded Handle(Market market, AddRunnersCommand command)
    {
        return new MarketRunnersAdded(
            market.Id,
            command.Runners.Select(r => new RunnerSnapshot(
                Guid.NewGuid(),
                r.Name,
                new PriceSnapshot(r.BackPrice.Price, r.BackPrice.Size),
                new PriceSnapshot(r.LayPrice.Price, r.LayPrice.Size))
            ).ToArray()
        );
    }

    private static IBackBetEvent Handle(Market market, SubmitBackBetCommand command)
    {
        return new BackBetPlaced();
    }
}
