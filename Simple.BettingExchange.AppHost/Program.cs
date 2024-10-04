var builder = DistributedApplication.CreateBuilder(args);

//var username = builder.AddParameter("username");
//var password = builder.AddParameter("password");

var db = builder
    .AddPostgres("db-betting", port: 5432)
    .WithImageTag("alpine")
    //.WithDataVolume()
    .WithPgAdmin(c => c.WithHostPort(5050))
    .WithHealthCheck()
    .AddDatabase("postgres");

builder.AddProject<Projects.Simple_BettingExchange_Api>("simple-bettingexchange-api")
    .WithReference(db)
    .WaitFor(db);


builder.Build().Run();
