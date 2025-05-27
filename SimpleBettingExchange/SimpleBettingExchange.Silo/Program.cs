using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Serialization;
using SimpleBettingExchange.Markets;
using Weasel.Core;

using var host = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        siloBuilder.AddCustomStorageBasedLogConsistencyProviderAsDefault();
        siloBuilder.AddMemoryGrainStorageAsDefault();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var env = context.HostingEnvironment;
        
        services.AddMarten(options =>
            {
                var schemaName = "PariSportifExchange";
                options.Connection(configuration.GetConnectionString("BettingExchange")!);
                options.DatabaseSchemaName = schemaName;

                options.UseSystemTextJsonForSerialization(
                    EnumStorage.AsString,
                    casing: Casing.CamelCase);

                if (env.IsDevelopment())
                {
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                }

                options.Events.StreamIdentity = StreamIdentity.AsGuid; // Utilise GUID pour les streams
    
                // options.Schema.For<MarketSummary>()
                //     .Identity(x => x.Id); // Et GUID pour les documents
    
                // options.Projections.Add<MarketListProjection>(ProjectionLifecycle.Async);
            })
            .ApplyAllDatabaseChangesOnStartup()
            .AddAsyncDaemon(DaemonMode.Solo);
    }).Build();


// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();
