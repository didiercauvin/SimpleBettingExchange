using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBettingExchange.Markets;

using var host = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        siloBuilder
            .AddCustomStorageBasedLogConsistencyProvider("MarketStore")
            .AddCustomStorageBasedLogConsistencyProviderAsDefault();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
    }).Build();


// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();
