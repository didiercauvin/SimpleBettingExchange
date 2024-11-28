﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBettingExchange.Markets;

using var host = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        siloBuilder
            .AddCustomStorageBasedLogConsistencyProviderAsDefault();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        //services.AddScoped<IMarketRepository, OrleansMarketRepository>();
    }).Build();


// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();
