using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Simple.BettingExchange.Api.Modules.Markets;
using Simple.BettingExchange.Api.Modules.Trading;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Host.UseWolverine(opts =>
{
    opts.Services.AddMarten(options =>
    {
        var schemaName = "IntroBettingExchange";
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


        //options.DisableNpgsqlLogging = true;
        //options.Projections.LiveStreamAggregation<Order>();
        //options.Projections.LiveStreamAggregation<Market>();

        options.Projections.Add<AllMarketOrdersProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<AllMarketsProjection>(ProjectionLifecycle.Async);


    })
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup()
    .AddAsyncDaemon(DaemonMode.Solo)
    .IntegrateWithWolverine();

    opts.ApplicationAssembly = typeof(Program).Assembly;

    // You want this maybe!
    opts.Policies.AutoApplyTransactions();

});

var app = builder.Build();

app.MapWolverineEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync
            <AllMarketsProjection>(CancellationToken.None);
    }
}

app.Run();
