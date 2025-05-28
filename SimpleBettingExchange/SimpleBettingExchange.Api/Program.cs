using System.Reflection;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Oakton;
using SimpleBettingExchange.Markets;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opts =>
{
    opts.Services.AddMarten(options =>
    {
        var schemaName = "PariSportifExchange";
        options.DatabaseSchemaName = schemaName;
        // Establish the connection string to your Marten database
        options.Connection(builder.Configuration.GetConnectionString("BettingExchange")!);

        // Specify that we want to use STJ as our serializer
        options.UseSystemTextJsonForSerialization(
            EnumStorage.AsString,
            casing: Casing.CamelCase);

        // If we're running in development mode, let Marten just take care
        // of all necessary schema building and patching behind the scenes
        if (builder.Environment.IsDevelopment())
        {
            options.AutoCreateSchemaObjects = AutoCreate.All;
        }
        
        // options.Schema.For<MarketSummary>();

        //options.DisableNpgsqlLogging = true;
        //options.Projections.LiveStreamAggregation<Pari>();
        // options.Projections.LiveStreamAggregation<Market>();

        //options.Projections.Add<PariProjection>(ProjectionLifecycle.Inline);
        //options.Projections.Add<PariShortProjection>(ProjectionLifecycle.Inline);
        //options.Projections.Add<AllMarketsProjection>(ProjectionLifecycle.Async);


    })
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup()
    .IntegrateWithWolverine();

    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    opts.Discovery.IncludeAssembly(Assembly.Load("SimpleBettingExchange.Markets"));
    
    // You want this maybe!
    opts.Policies.AutoApplyTransactions();

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();

builder.Host.UseOrleansClient(static builder =>
{
    builder.UseLocalhostClustering();
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
//builder.Services.AddScoped<IMarketRepository, OrleansMarketRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseRouting();
app.MapWolverineEndpoints();
// app.UseCreateMarketEndpoint();
// app.UseChangeMarketNameEndpoint();
// app.UseAddRunnersEndpoint();
// app.UseSuspendMarketEndpoint();
// app.UseResumeMarketEndpoint();
// app.UseCloseMarketEndpoint();


return await app.RunOaktonCommands(args);